using ErrorOr;
using h.Contracts;
using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Server.Entities.MultiplayerGames;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSessionHub : Hub<IMultiplayerGameSessionHubClient>
{
    private readonly IHubUserIdMappingService<MultiplayerGameSessionHub> _userIdMappingService;
    private readonly IMultiplayerGameSessionService _gameSessionService;
    private readonly AppDbContext _db;

    public MultiplayerGameSessionHub(IMultiplayerGameSessionService gameSessionService, IHubUserIdMappingService<MultiplayerGameSessionHub> userIdMappingService, AppDbContext db)
    {
        _gameSessionService = gameSessionService;
        _userIdMappingService = userIdMappingService;
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
        var userId = Context.User.GetUserId();
        _userIdMappingService.Add(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from mapping
        if(Context.User is {Identity: { IsAuthenticated: true } })
        {
            var userId = Context.User.GetUserId();
            _userIdMappingService.Remove(userId);
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

        var userId = Context.User.GetUserId();

        var result = _gameSessionService.ConfirmPlayerLoaded(gameId, userId);
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

        await Clients.Clients(
            game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub>.UserNotPresentException(userId))
            )
            .GameStarted(new(
                gameId,
                gameStartResult.PlayerSymbols,
                gameStartResult.StartingPlayer
            )
        );
    }

    public async Task<ErrorOr<Guid?>> PlaceSymbol(Guid gameId, Int2 atPos)
    {
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return default;
        }

        var userId = Context.User.GetUserId();

        var result = _gameSessionService.PlaceSymbolAsyncAndMoveTurn(gameId, userId, atPos);

        if(result.IsError)
            return result;

        var game = _gameSessionService.GetGame(gameId);
        if(game is null)
            throw new NullReferenceException("Game not found after successful move"); // Should never happen

        var nextPlayerOnTurn = result.Value;
        var connectionIds = game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub>.UserNotPresentException(userId))
            .ToArray();

        // Notify players about the move
        await Clients.Clients(connectionIds)
            .PlayerMadeMove(new(
                userId,
                atPos,
                game.PlayerSymbols[userId],
                nextPlayerOnTurn
            ));

        // No next player on turn, the game is over
        if(nextPlayerOnTurn is null)
        {
            // Get end result
            var endResult = _gameSessionService.GetEndResult(gameId);
            if(endResult is null)
                throw new NullReferenceException("No next player on turn but end results are null"); // Should never happen

            var player1Id = game.Players.ElementAt(0);
            var player2Id = game.Players.ElementAt(1);
            var player1 = await _db.UsersDbSet.AsTracking().FirstOrDefaultAsync(u => u.Uuid == player1Id)
                ?? throw new SharedErrors.User.UserNotFoundException();
            var player2 = await _db.UsersDbSet.AsTracking().FirstOrDefaultAsync(u => u.Uuid == player2Id)
                ?? throw new SharedErrors.User.UserNotFoundException();

            // Save statistics
            // Todo: calculate elo
            if (endResult.IsDraw)
            {
                player1.DrawAmount++;
                player2.DrawAmount++;
            } else if(endResult.WinnerId == player1Id)
            {
                player1.WinAmount++;
                player2.LossAmount++;
            }
            else
            {
                player2.WinAmount++;
                player1.LossAmount++;
            }

            // Save game
            var finishedRankedGame = new FinishedRankedGame()
            {
                LastBoardState = game.Board,
                PlayedAt = game.CreatedAtUtc.UtcDateTime,
                Player1Id = player1Id,
                Player2Id = player2Id,
                Player1Symbol = game.PlayerSymbols[player1Id],
                Player2Symbol = game.PlayerSymbols[player2Id],
                Player1RemainingTimer = game.GetRemainingTime(player1Id),
                Player2RemainingTimer = game.GetRemainingTime(player2Id),
                IsDraw = endResult.IsDraw,
                WinnerId = endResult.WinnerId
            };

            _db.FinishedRankedGames.Add(finishedRankedGame);
            // Todo: what the fuck do we do if this fails?
            await _db.SaveChangesAsync(); // Save, so we have id for finished game

            _db.UserToFinishedRankedGames.AddRange([
                new() {
                    UserId = player1Id,
                    FinishedRankedGameId = finishedRankedGame.Id
                },
                new() {
                    UserId = player2Id,
                    FinishedRankedGameId = finishedRankedGame.Id
                }
            ]);          
            // Todo: what the fuck do we do if this fails?
            await _db.SaveChangesAsync(); // Save finished game to user mappings

            // Notify players about the game over
            await Clients.Clients(connectionIds)
                .GameEnded(new(
                    endResult.IsDraw,
                    endResult.WinnerId
                ));
        }

        return result;
    }

    // Todo: leave game
}
