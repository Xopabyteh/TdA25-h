using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static h.Server.Infrastructure.MultiplayerGames.MultiplayerGameStatisticsService;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSessionHub : Hub<IMultiplayerGameSessionHubClient>
{
    private readonly IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity> _userIdMappingService;
    private readonly IMultiplayerGameSessionService _gameSessionService;
    private readonly MultiplayerGameStatisticsService _multiplayerGameStatisticsService;
    private readonly AppDbContext _db;
    private readonly ILogger<MultiplayerGameSessionHub> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MultiplayerGameSessionHub(
        IMultiplayerGameSessionService gameSessionService,
        IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity> userIdMappingService,
        MultiplayerGameStatisticsService multiplayerGameStatisticsService,
        AppDbContext db,
        ILogger<MultiplayerGameSessionHub> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _gameSessionService = gameSessionService;
        _userIdMappingService = userIdMappingService;
        _multiplayerGameStatisticsService = multiplayerGameStatisticsService;
        _db = db;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task OnConnectedAsync()
    {
        if(Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return;
        }

        // Add the connection ID to the mapping service
        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);

        _userIdMappingService.Add(Context.ConnectionId, identity);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from mapping
        if(Context.User is not {Identity: { IsAuthenticated: true } })
        {
            await base.OnDisconnectedAsync(exception);
            return;
        }

        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
        _userIdMappingService.Remove(identity);
        
        // If player was part of a game, remove him and consider the other one as winner
        var gameOfUser = _gameSessionService.GetGameByPlayer(identity);
        if(gameOfUser is not null
            && !gameOfUser.GameEnded
            && gameOfUser.GameStarted)
        {
            await HandlePlayerDisconnectMidGame(gameOfUser, identity);
        }


        await base.OnDisconnectedAsync(exception);
    }

    private async Task HandlePlayerDisconnectMidGame(MultiplayerGameSession game, MultiplayerGameUserIdentity disconnectedPlayer)
    {
        var otherPlayer = game.Players.Single(p => p != disconnectedPlayer);
        var otherPlayerConnId = _userIdMappingService.GetConnectionId(otherPlayer)
            ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity>.UserNotPresentException(otherPlayer);
        
        // End the game with results
        _gameSessionService.EndGameEarly(game.Id, otherPlayer);

        var endResult = _gameSessionService.GetEndResult(game.Id)
            ?? throw new NullReferenceException("End result is null after ending the game early");

        var updateResult = await _multiplayerGameStatisticsService.UpdateAndSavePlayerStatisticsAsync(game);
        await HandleGameEndedAsync(game, endResult, isRevangePossible: false, updateResult);

        // Kill session (it wont be possible to revange anymore)
        _gameSessionService.KillSession(game.Id);
    }

    public async Task ConfirmLoaded(Guid gameId)
    {
        // Todo: timeout if not all players confirm
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {   
            Context.Abort();
            return;
        }

        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);

        var result = _gameSessionService.ConfirmPlayerLoaded(gameId, identity);
        if(result.IsError)
            return;

        // -> confirmed successfully, check if every1 is ready
        if(!result.Value)
            return;

        // -> Every1 is ready, start the game, notify clients
        var game = _gameSessionService.GetGame(gameId);
        if(game is null)
            throw new NullReferenceException("Game not found after all players confirmed"); // Should never happen

        game.OnClockRanOutAndGameEnded += async args =>
        {
            // Try catch because of async void
            try
            {
                var endResult = _gameSessionService.GetEndResult(args.Game.Id);
                if (endResult is null)
                    throw new NullReferenceException("End result is null after clock ran out");

                // Notify players about the game over
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var multiplayerGameStatisticsService = scope.ServiceProvider.GetRequiredService<MultiplayerGameStatisticsService>();
                var updateResult = await multiplayerGameStatisticsService.UpdateAndSavePlayerStatisticsAsync(game);
                
                var losingPlayer = args.Player;

                await HandleGameEndedAsync(args.Game, endResult, isRevangePossible: true, updateResult);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Error while handling clock ran out event");
            }
        };

        // Start game
        var gameStartResult = _gameSessionService.StartGame(gameId);

        // Notify about game details
        var identitiesAndConnections = game.Players.Select(identity => 
            (
                identity,
                connectionId: _userIdMappingService.GetConnectionId(identity)
            ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity>.UserNotPresentException(identity)
            )
        );

        var nonGuestUserIds = game.Players.Where(u => !u.IsGuest).Select(u => u.UserId!.Value).ToArray();
        var nonGuestUsersInGame = await _db.UsersDbSet
            .Where(u => nonGuestUserIds.Contains(u.Uuid))
            .ToArrayAsync();

        var playersDto = game.Players
            .Select(u => new MultiplayerGameSessionUserDetailDTO(
                MapToDto(u),
                game.PlayerSymbols[u],
                u.Name,
                nonGuestUsersInGame.FirstOrDefault(user => user.Uuid == u.UserId!.Value)?.Elo.Rating,
                game.TimerLength
            ))
            .ToList()
            .AsReadOnly();

        // Tell each client about his identity and about game details
        foreach(var identityAndConnection in identitiesAndConnections)
        {
            await Clients.Client(identityAndConnection.connectionId)
                .GameStarted(new(
                    gameId,
                    identityAndConnection.identity.SessionId,
                    MapToDto(gameStartResult.StartingPlayer),
                    playersDto
                ));
        }
    }

    public async Task PlaceSymbol(Guid gameId, Int2 atPos)
    {
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return;
        }

        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
        var result = _gameSessionService.PlaceSymbolAsyncAndMoveTurn(gameId, identity, atPos);
        if(result.IsError)
            return;

        var game = _gameSessionService.GetGame(gameId);
        if(game is null)
            throw new NullReferenceException("Game not found after successful move"); // Should never happen

        var nextPlayerOnTurn = result.Value;
        var connectionIds = game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity>.UserNotPresentException(identity))
            .ToArray();

        var playerRemainingClockTimes = game.Players
            .Select(p => new KeyValuePair<Guid, TimeSpan>(
                p.SessionId,
                game.GetRemainingTime(p)
            ))
            .ToArray();

        // Notify players about the move
        await Clients.Clients(connectionIds)
            .PlayerMadeAMove(new PlayerMadeAMoveResponse(
                MapToDto(identity),
                atPos,
                game.PlayerSymbols[identity],
                MapToDto(nextPlayerOnTurn),
                playerRemainingClockTimes
            ));

        // No next player on turn, the game is over
        if(nextPlayerOnTurn is null)
        {
            // Get end result
            var endResult = _gameSessionService.GetEndResult(gameId);
            if(endResult is null)
                throw new NullReferenceException("No next player on turn but end results are null"); // Should never happen
            
            var updateResult = await _multiplayerGameStatisticsService.UpdateAndSavePlayerStatisticsAsync(game);

            await HandleGameEndedAsync(game, endResult, isRevangePossible: true, updateResult);
        }
    }

    private async Task HandleGameEndedAsync(
        MultiplayerGameSession game,
        MultiplayerGameSessionEndResult endResult,
        bool isRevangePossible,
        StatisticsUpdateResult statsUpdateResult)
    {

        // Notify players about the game over
        foreach(var player in game.Players)
        {
            var connId = _userIdMappingService.GetConnectionId(player);
            if(connId is null)
                continue;

            await Clients.Client(connId)
                .GameEnded(new(
                    endResult.IsDraw,
                    MapToDto(endResult.WinnerId),
                    statsUpdateResult.DidUpdateElo,
                    statsUpdateResult.OldElos?.Single(kvp => kvp.Key == player.UserId!.Value).Value.Rating ?? default,
                    statsUpdateResult.NewElos?.Single(kvp => kvp.Key == player.UserId!.Value).Value.Rating ?? default,
                    isRevangePossible
                ));
        }
    }

    public async Task RequestRevange(Guid gameId)
    {
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return;
        }

        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
        var game = _gameSessionService.GetGame(gameId);
        if (game is null)
            throw new NullReferenceException("Game not found when requiring revange match"); // Should never happen

        if(!game.GameEnded)
            return;

        // Get conn ids
        var connectionIds = game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity>.UserNotPresentException(identity))
            .ToArray();

        // Request revange
        var allAccepted = game.RequestRevange(identity);
        
        // Notify about revange
        await Clients.Clients(connectionIds)
            .PlayerRequestedRevange(MapToDto(identity));

        if (!allAccepted)
            return;

        // Every1 accepted -> Start new revange match
        var newGame = _gameSessionService.CreateRevangeSession(game);

        // Notify players about the new game
        await Clients.Clients(connectionIds)
            .NewRevangeGameSessionCreated(newGame.Id);
    }

    public async Task Surrender(Guid gameId)
    {
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {   
            Context.Abort();
            return;
        }

        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
        var game = _gameSessionService.GetGame(gameId);
        if (game is null)
            throw new NullReferenceException("Game not found when surrendering"); // Should never happen

        var connectionIds = game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity>.UserNotPresentException(identity))
            .ToArray();

        // End the game with results
        var otherPlayer = game.Players.Single(p => p != identity);
        _gameSessionService.EndGameEarly(gameId, otherPlayer);

        var endResult = _gameSessionService.GetEndResult(gameId)
            ?? throw new NullReferenceException("End result is null after ending the game early");

        var updateResult = await _multiplayerGameStatisticsService.UpdateAndSavePlayerStatisticsAsync(game);
        await HandleGameEndedAsync(game, endResult, isRevangePossible: true, updateResult);
    }

    // Helper methods
    private MultiplayerGameUserIdentityDTO MapToDto(MultiplayerGameUserIdentity identity)
        => new(
            identity.SessionId,
            identity.IsGuest
        );

    private MultiplayerGameUserIdentityDTO? MapToDto(MultiplayerGameUserIdentity? identity)
        => identity is null
        ? null
        : new(
            identity.Value.SessionId,
            identity.Value.IsGuest
        );
}
