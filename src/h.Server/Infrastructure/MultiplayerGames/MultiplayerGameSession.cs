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

    public Guid PlayerOnTurn => Players.ElementAt(_playerOnTurnIndex);
    private int _playerOnTurnIndex;
    public MultiplayerGameSession(
        Guid id,
        IReadOnlyCollection<Guid> players,
        GameBoard board,
        Dictionary<Guid, GameSymbol> playerSymbols,
        int playerOnTurnIndex)
    {
        Id = id;
        Players = players;
        Board = board;
        PlayerSymbols = playerSymbols;
        _playerOnTurnIndex = playerOnTurnIndex;
        
        PlayerTimers = Players.ToDictionary(
            keySelector: pId => pId, 
            elementSelector: pId => new Stopwatch());
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
    }
}
