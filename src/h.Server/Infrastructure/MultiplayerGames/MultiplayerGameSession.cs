using h.Primitives.Games;
using h.Server.Entities.Games;
using System.Diagnostics;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameSession
{
    public Guid Id { get; init; }
    public IReadOnlyCollection<MultiplayerGameUserIdentity> Players { get; init; }
    public GameBoard Board { get; init; }

    /// <summary>
    /// Key: PlayerId, Value: Symbol
    /// </summary>
    public Dictionary<MultiplayerGameUserIdentity, GameSymbol> PlayerSymbols { get; init; }
    public Dictionary<MultiplayerGameUserIdentity, Stopwatch> PlayerTimers { get; init; }
    public List<MultiplayerGameUserIdentity> ReadyPlayers { get; init; } = new(2);
    public List<MultiplayerGameUserIdentity> PlayersRequestingRevange { get; init; } = new(2);
    public MultiplayerGameUserIdentity StartingPlayer { get; init; }
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

    public MultiplayerGameUserIdentity PlayerOnTurn => Players.ElementAt(_playerOnTurnIndex);
    private int _playerOnTurnIndex;

    /// <summary>
    /// How much time each player has for the whole game.
    /// "Šachové hodiny"
    /// </summary>
    public TimeSpan TimerLength { get; init;}

    public MultiplayerGameSession(
        Guid id,
        IReadOnlyCollection<MultiplayerGameUserIdentity> players,
        GameBoard board,
        Dictionary<MultiplayerGameUserIdentity, GameSymbol> playerSymbols,
        int playerOnTurnIndex,
        DateTimeOffset createdAtUtc,
        TimeSpan timerLength)
    {
        Id = id;
        Players = players;
        Board = board;
        PlayerSymbols = playerSymbols;

        PlayerTimers = Players.ToDictionary(
            keySelector: pId => pId,
            elementSelector: pId => new Stopwatch());
        CreatedAtUtc = createdAtUtc;
        TimerLength = timerLength;
        
        _playerOnTurnIndex = playerOnTurnIndex;
        StartingPlayer = PlayerOnTurn;
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

    public TimeSpan GetRemainingTime(MultiplayerGameUserIdentity ofPlayer)
    {
        var timer = PlayerTimers[ofPlayer];
        return TimerLength - timer.Elapsed;
    }

    /// <summary>
    /// Request a revange match.
    /// Returns true when all players have requested a revange.
    /// </summary>
    /// <exception cref="GameNotEndedYetException"></exception>
    public bool RequestRevange(MultiplayerGameUserIdentity byPlayer)
    {
        if (!GameEnded)
            throw new GameNotEndedYetException();

        if(!Players.Contains(byPlayer))
            throw new ArgumentException("Player is not in the game.", nameof(byPlayer));

        PlayersRequestingRevange.Add(byPlayer);

        return PlayersRequestingRevange.Count == Players.Count;
    }
    public class GameNotEndedYetException : Exception
    {
        public GameNotEndedYetException() : base("Game has not ended yet.")
        {
        }
    }
}
