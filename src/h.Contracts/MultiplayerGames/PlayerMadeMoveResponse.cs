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
public readonly record struct PlayerMadeMoveResponse(
    MultiplayerGameUserIdentityDTO PlayerId,
    Int2 Position,
    GameSymbol Symbol,
    MultiplayerGameUserIdentityDTO? NextPlayerOnTurn,
    KeyValuePair<Guid, TimeSpan>[] PlayerRemainingClockTimes
);
