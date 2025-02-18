using ErrorOr;
using h.Contracts;
using h.Contracts.MultiplayerGames;
using h.Primitives;
using h.Server.Infrastructure.Auth;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSessionHub : Hub<IMultiplayerGameSessionHubClient>
{
    private readonly IHubUserIdMappingService<MultiplayerGameSessionHub> _userIdMappingService;
    private readonly IMultiplayerGameSessionService _gameSessionService;

    public MultiplayerGameSessionHub(IMultiplayerGameSessionService gameSessionService, IHubUserIdMappingService<MultiplayerGameSessionHub> userIdMappingService)
    {
        _gameSessionService = gameSessionService;
        _userIdMappingService = userIdMappingService;
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
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {   
            Context.Abort();
            return;
        }

        var userId = Context.User.GetUserId();

        var result = await _gameSessionService.ConfirmPlayerLoadedAsync(gameId, userId);
        if(result.IsError)
            return;

        // -> confirmed successfully, check if every1 is ready
        if(!result.Value)
            return;

        // -> Every1 is ready, start the game, notify clients
        var game = await _gameSessionService.GetGameAsync(gameId);
        if(game is null)
            throw new NullReferenceException("Game not found after all players confirmed"); // Should never happen

        var gameStartResult = await _gameSessionService.StartGameAsync(gameId);

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

    public async Task<ErrorOr<Guid>> PlaceSymbol(Guid gameId, Int2 atPos)
    {
        if (Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return default;
        }

        var userId = Context.User.GetUserId();

        var result = await _gameSessionService.PlaceSymbolAsyncAndMoveTurn(gameId, userId, atPos);

        if(result.IsError)
            return result;

        // Notify players about the move
        var game = await _gameSessionService.GetGameAsync(gameId);
        if(game is null)
            throw new NullReferenceException("Game not found after successful move"); // Should never happen

        await Clients.Clients(
            game.Players
            .Select(userId => _userIdMappingService.GetConnectionId(userId)
                ?? throw IHubUserIdMappingService<MultiplayerGameSessionHub>.UserNotPresentException(userId))
            )
            .PlayerMadeMove(new(
                userId,
                atPos,
                game.PlayerSymbols[userId]
            ));

        return result;
    }
}
