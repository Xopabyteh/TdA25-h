using h.Primitives;
using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

/// <summary>
/// </summary>
/// <param name="NextPlayerOnTurn">null when there is no next player - I.E. the game is over</param>
/// <param name="PlayerRemainingClockTimes">
/// Key: sessionId
/// Value: remaining time on clock
/// Used for syncing player timers on client, after a player makes a move
/// </param>
// Exclude from linking (trimming)
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class PlayerMadeAMoveResponse
{
    public MultiplayerGameUserIdentityDTO PlayerId { get; init; }
    public Int2 Position { get; init; }
    public GameSymbol Symbol { get; init; }
    public MultiplayerGameUserIdentityDTO? NextPlayerOnTurn { get; init; }
    public KeyValuePair<Guid, TimeSpan>[] PlayerRemainingClockTimes { get; init; }

    public PlayerMadeAMoveResponse(
        MultiplayerGameUserIdentityDTO playerId,
        Int2 position,
        GameSymbol symbol,
        MultiplayerGameUserIdentityDTO? nextPlayerOnTurn,
        KeyValuePair<Guid, TimeSpan>[] playerRemainingClockTimes)
    {
        PlayerId = playerId;
        Position = position;
        Symbol = symbol;
        NextPlayerOnTurn = nextPlayerOnTurn;
        PlayerRemainingClockTimes = playerRemainingClockTimes;
    }
}
