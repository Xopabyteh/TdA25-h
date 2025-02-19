using ErrorOr;
using h.Contracts;
using h.Primitives;
using h.Primitives.Games;
using h.Server.Entities.Games;
using System.Collections.Concurrent;

namespace h.Server.Infrastructure.MultiplayerGames;

public class InMemoryMultiplayerGameSessionService : IMultiplayerGameSessionService
{
    private readonly ConcurrentDictionary<Guid, MultiplayerGameSession> _gameSessions
        = new(concurrencyLevel: -1, capacity: 30);


    public Task<MultiplayerGameSession> CreateGameSessionAsync(IReadOnlyCollection<Guid> players)
    {
        var gameId = Guid.NewGuid();
        var board = GameBoard.CreateNew();

        // Since we always only have 2 players (for now) and two symbols,
        // we can assign first player to X and second to O
        var playerSymbols = new Dictionary<Guid, GameSymbol>
        {
            [players.First()] = GameSymbol.X,
            [players.Last()] = GameSymbol.O
        };

        // Pick at random
        // Todo: change to be the other player when doing revenge matches
        var playerOnTurnIndex = new Random().Next(0, players.Count);

        var gameSession = new MultiplayerGameSession(
            gameId,
            players,
            board,
            playerSymbols,
            playerOnTurnIndex
        );

        _gameSessions[gameId] = gameSession;
        
        return Task.FromResult(gameSession);
    }

    public Task<ErrorOr<bool>> ConfirmPlayerLoadedAsync(Guid gameId, Guid playerId)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if(!didFindGame)
            return Task.FromResult(Error.NotFound(description: "Game not found").ToErrorOr<bool>()); // Turn into shared error if needed
    
        gameSession!.ReadyPlayers.Add(playerId);
        
        return Task.FromResult((gameSession.ReadyPlayers.Count == gameSession.Players.Count).ToErrorOr());
    }

    public Task<MultiplayerGameSession?> GetGameAsync(Guid byGameId)
    {
        var didFindGame = _gameSessions.TryGetValue(byGameId, out var gameSession);
        return didFindGame
            ? Task.FromResult(gameSession)
            : Task.FromResult<MultiplayerGameSession?>(null);
    }

    public Task<ErrorOr<Guid>> PlaceSymbolAsyncAndMoveTurn(Guid gameId, Guid byPlayerId, Int2 atPos)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if(!didFindGame)
            return Task.FromResult(Error.NotFound(description: "Game not found").ToErrorOr<Guid>()); // Turn into shared error if needed
    
        var isPlayerOnTurn = gameSession!.PlayerOnTurn == byPlayerId;
        if (!isPlayerOnTurn)
            return Task.FromResult(Error.Forbidden(description: "Not your turn").ToErrorOr<Guid>()); // Turn into shared error if needed

        var symbolAtPlace = gameSession!.Board.GetSymbolAt(atPos);
        if(symbolAtPlace != GameSymbol.None)
            return Task.FromResult(Error.Conflict(description: "Space already occupied").ToErrorOr<Guid>()); // Turn into shared error if needed

        var playerSymbol = gameSession!.PlayerSymbols[byPlayerId];
        gameSession!.Board.SetSymbolAt(atPos, playerSymbol);
        gameSession!.SetNextPlayerOnTurn();

        return Task.FromResult(byPlayerId.ToErrorOr());
    }

    public Task<(Guid StartingPlayer, KeyValuePair<Guid, GameSymbol>[] PlayerSymbols)> StartGameAsync(Guid gameId)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if (!didFindGame)
            throw new SharedErrors.MultiplayerGames.GameNotFoundException();

        gameSession!.StartGame();

        return Task.FromResult((
            gameSession!.PlayerOnTurn,
            gameSession!.PlayerSymbols.ToArray()
        ));
    }
}