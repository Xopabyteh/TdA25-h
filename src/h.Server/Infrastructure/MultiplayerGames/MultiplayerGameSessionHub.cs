using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Primitives.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSessionHub : Hub<IMultiplayerGameSessionHubClient>
{
    private readonly IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity> _userIdMappingService;
    private readonly IMultiplayerGameSessionService _gameSessionService;
    private readonly MultiplayerGameStatisticsService _multiplayerGameStatisticsService;
    private readonly AppDbContext _db;

    public MultiplayerGameSessionHub(
        IMultiplayerGameSessionService gameSessionService,
        IHubUserIdMappingService<MultiplayerGameSessionHub, MultiplayerGameUserIdentity> userIdMappingService,
        MultiplayerGameStatisticsService multiplayerGameStatisticsService,
        AppDbContext db)
    {
        _gameSessionService = gameSessionService;
        _userIdMappingService = userIdMappingService;
        _multiplayerGameStatisticsService = multiplayerGameStatisticsService;
        _db = db;
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

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from mapping
        if(Context.User is {Identity: { IsAuthenticated: true } })
        {
            var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
            _userIdMappingService.Remove(identity);
        }

        return base.OnDisconnectedAsync(exception);
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

        var gameStartResult = _gameSessionService.StartGame(gameId);

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
            .PlayerMadeMove(new(
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

            await _multiplayerGameStatisticsService.UpdateAndSavePlayerStatisticsAsync(game);

            // Notify players about the game over
            await Clients.Clients(connectionIds)
                .GameEnded(new(
                    endResult.IsDraw,
                    MapToDto(endResult.WinnerId)
                ));
        }
    }

    // Todo: leave game

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
