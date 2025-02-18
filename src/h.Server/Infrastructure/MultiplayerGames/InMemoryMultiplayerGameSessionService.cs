using ErrorOr;
using h.Primitives;
using h.Primitives.Games;
using System.Collections.Concurrent;

namespace h.Server.Infrastructure.MultiplayerGames;

public class InMemoryMultiplayerGameSessionService : IMultiplayerGameSessionService
{
    private readonly ConcurrentDictionary<Guid, MultiplayerGameSession> _gameSessions
        = new(concurrencyLevel: -1, capacity: 30);

    public Task<MultiplayerGameSession> CreateGameSessionAsync(IReadOnlyCollection<Guid> players)
    {
        var gameId = Guid.NewGuid();

        var gameSession = new MultiplayerGameSession(gameId, players);
        _gameSessions[gameId] = gameSession;
        
        return Task.FromResult(gameSession);
    }

    public Task<ErrorOr<bool>> ConfirmPlayerLoadedAsync(Guid gameId, Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task<MultiplayerGameSession> GetGameAsync(Guid byGameId)
    {
        throw new NotImplementedException();
    }

    public Task<ErrorOr<Unit>> PlaceSymbolAsync(Guid gameId, Guid byPlayerId, Int2 atPos)
    {
        throw new NotImplementedException();
    }

    public Task<(Guid StartingPlayer, KeyValuePair<Guid, GameSymbol>[] PlayerSymbols)> StartGameAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }
}
