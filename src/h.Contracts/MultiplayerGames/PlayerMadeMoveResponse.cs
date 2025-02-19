using h.Primitives;
using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

/// <param name="NextPlayerOnTurn">null when there is no next player - I.E. the game is over</param>
public readonly record struct PlayerMadeMoveResponse(
    Guid PlayerId,
    Int2 Position,
    GameSymbol Symbol,
    Guid? NextPlayerOnTurn
);
