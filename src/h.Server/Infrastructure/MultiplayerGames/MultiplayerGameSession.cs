using h.Primitives.Games;
using h.Server.Entities.Games;
using System.Diagnostics;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSession
{
    public Guid Id { get; init; }
    public IReadOnlyCollection<Guid> Players { get; init; }
    public GameBoard Board { get; init; }

    /// <summary>
    /// Key: PlayerId, Value: Symbol
    /// </summary>
    public Dictionary<Guid, GameSymbol> PlayerSymbols { get; init; }
    public Dictionary<Guid, Stopwatch> PlayerTimers { get; init; }
    public List<Guid> ReadyPlayers { get; init; } = new(2);

    /// <summary>
    /// Whether the game has started at any time.
    /// May be true even if the game has ended - because it has started at some point.
    /// </summary>
    public bool GameStarted { get; private set; }

    /// <summary>
    /// Whether the game has ended.
    /// </summary>
    public bool GameEnded { get; private set; }
    public MultiplayerGameSessionEndResult? EndResult { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; init; }

    public Guid PlayerOnTurn => Players.ElementAt(_playerOnTurnIndex);
    private int _playerOnTurnIndex;

    /// <summary>
    /// How much time each player has for the whole game.
    /// "Šachové hodiny"
    /// </summary>
    private TimeSpan _timerLength;

    public MultiplayerGameSession(
        Guid id,
        IReadOnlyCollection<Guid> players,
        GameBoard board,
        Dictionary<Guid, GameSymbol> playerSymbols,
        int playerOnTurnIndex,
        DateTimeOffset createdAtUtc,
        TimeSpan timerLength)
    {
        Id = id;
        Players = players;
        Board = board;
        PlayerSymbols = playerSymbols;
        _playerOnTurnIndex = playerOnTurnIndex;

        PlayerTimers = Players.ToDictionary(
            keySelector: pId => pId,
            elementSelector: pId => new Stopwatch());
        CreatedAtUtc = createdAtUtc;
        _timerLength = timerLength;
    }

    /// <summary>
    /// Moves the turn to the next player.
    /// Stops timer of the current player and starts timer of the next player.
    /// </summary>
    public void SetNextPlayerOnTurn()
    {
        var currentTimer = PlayerTimers[PlayerOnTurn];
        currentTimer.Stop();
        
        _playerOnTurnIndex = (_playerOnTurnIndex + 1) % Players.Count;
        
        var nextPlayerTimer = PlayerTimers[PlayerOnTurn];
        nextPlayerTimer.Start();
    }

    /// <summary>
    /// Starts the game: 
    /// <list type="bullet">
    /// Start the timer of the player on turn.
    /// </list>
    /// </summary>
    public void StartGame()
    {
        PlayerTimers[PlayerOnTurn].Start();
        GameStarted = true;
    }

    public void EndGame(MultiplayerGameSessionEndResult endResult)
    {
        PlayerTimers[PlayerOnTurn].Stop();
        GameEnded = true;
        EndResult = endResult;
    }

    public TimeSpan GetRemainingTime(Guid ofPlayer)
    {
        var timer = PlayerTimers[ofPlayer];
        return _timerLength - timer.Elapsed;
    }
}
