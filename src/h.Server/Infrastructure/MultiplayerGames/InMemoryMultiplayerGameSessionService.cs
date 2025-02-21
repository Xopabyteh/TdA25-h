﻿using ErrorOr;
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

    private readonly TimeProvider _timeProvider;

    public InMemoryMultiplayerGameSessionService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public MultiplayerGameSession CreateGameSession(
        IReadOnlyList<MultiplayerGameUserIdentity> players,
        MultiplayerGameUserIdentity? forcedStartingPlayerId = null)
    {
        var gameId = Guid.NewGuid();
        var board = GameBoard.CreateNew();

        // Since we always only have 2 players (for now) and two symbols,
        // we can assign first player to X and second to O
        var playerSymbols = new Dictionary<MultiplayerGameUserIdentity, GameSymbol>
        {
            [players.First()] = GameSymbol.X,
            [players.Last()] = GameSymbol.O
        };

        // Who starts?
        int playerOnTurnIndex;
        if(forcedStartingPlayerId is not null)
        {
            // Pick forced
            var forcedIndex = players.ToList().IndexOf(forcedStartingPlayerId.Value);
            
            if (forcedIndex == -1)
                throw new Exception("Forced player not found in players list");

            playerOnTurnIndex = forcedIndex;
        }
        else
        {
            // Pick at random
            playerOnTurnIndex = new Random().Next(0, players.Count);
        }

        var gameSession = new MultiplayerGameSession(
            gameId,
            players,
            board,
            playerSymbols,
            playerOnTurnIndex,
            _timeProvider.GetUtcNow(),
            TimeSpan.FromSeconds(8) // Todo: moved to shared config
        );

        _gameSessions[gameId] = gameSession;
        
        return gameSession;
    }

    public ErrorOr<bool> ConfirmPlayerLoaded(Guid gameId, MultiplayerGameUserIdentity playerId)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if(!didFindGame)
            return Error.NotFound(description: "Game not found"); // Turn into shared error if needed
    
        gameSession!.ReadyPlayers.Add(playerId);
        
        return gameSession.ReadyPlayers.Count == gameSession.Players.Count;
    }

    public MultiplayerGameSession? GetGame(Guid byGameId)
    {
        var didFindGame = _gameSessions.TryGetValue(byGameId, out var gameSession);
        return didFindGame
            ? gameSession
            : null;
    }

    public ErrorOr<MultiplayerGameUserIdentity?> PlaceSymbolAsyncAndMoveTurn(Guid gameId, MultiplayerGameUserIdentity byPlayerId, Int2 atPos)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if (!didFindGame)
            return Error.NotFound(description: "Game not found"); // Turn into shared error if needed

        var isPlayerOnTurn = gameSession!.PlayerOnTurn == byPlayerId;
        if (!isPlayerOnTurn)
            return Error.Forbidden(description: "Not your turn"); // Turn into shared error if needed

        var symbolAtPlace = gameSession!.Board.GetSymbolAt(atPos);
        if (symbolAtPlace != GameSymbol.None)
            return Error.Conflict(description: "Space already occupied"); // Turn into shared error if needed

        var playerSymbol = gameSession!.PlayerSymbols[byPlayerId];
        gameSession!.Board.SetSymbolAt(atPos, playerSymbol);
        
        // Is game over?
        var isWinningSymbol = gameSession!.Board.IsWinningSymbol(atPos, playerSymbol);
        var isDraw = gameSession!.Board.IsDraw();
        if (!isWinningSymbol && !isDraw)
        {
            // -> Game not over
            gameSession!.SetNextPlayerOnTurn();
            return (MultiplayerGameUserIdentity?)gameSession!.PlayerOnTurn;
        }

        // Game over 
        gameSession.EndGame(new(
            isDraw,
            isWinningSymbol 
                ? byPlayerId 
                : null
        ));

        return (MultiplayerGameUserIdentity?)null; // No next player on turn
    }

    public (MultiplayerGameUserIdentity StartingPlayer, KeyValuePair<MultiplayerGameUserIdentity, GameSymbol>[] PlayerSymbols) StartGame(
        Guid gameId)
    {
        var didFindGame = _gameSessions.TryGetValue(gameId, out var gameSession);
        if (!didFindGame)
            throw new SharedErrors.MultiplayerGames.GameNotFoundException();

        gameSession!.StartGame();

        return (
            gameSession!.PlayerOnTurn,
            gameSession!.PlayerSymbols.ToArray()
        );
    }
    public MultiplayerGameSessionEndResult? GetEndResult(Guid gameId)
    {
        var gameSession = _gameSessions[gameId];
        if(gameSession is null)
            throw new SharedErrors.MultiplayerGames.GameNotFoundException();

        if (!gameSession.GameEnded)
            return null;

        return gameSession.EndResult;
    }

    public MultiplayerGameSession CreateGameSession(IReadOnlyList<Guid> playerIds, Guid? forcedStartingPlayerId = null)
    {
        return CreateGameSession(
            playerIds.Select(MultiplayerGameUserIdentity.FromUserId).ToList(),
            forcedStartingPlayerId: forcedStartingPlayerId is null
                ? null
                : MultiplayerGameUserIdentity.FromUserId(forcedStartingPlayerId.Value)
        );
    }
}