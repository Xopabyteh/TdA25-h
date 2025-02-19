using h.Primitives;
using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

/// <summary>
/// </summary>
/// <param name="NextPlayerOnTurn">null when there is no next player - I.E. the game is over</param>
public readonly record struct PlayerMadeMoveResponse(
    MultiplayerGameUserIdentityDTO PlayerId,
    Int2 Position,
    GameSymbol Symbol,
    MultiplayerGameUserIdentityDTO? NextPlayerOnTurn
);
