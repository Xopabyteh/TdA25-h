using h.Primitives;
using h.Primitives.Games;

namespace h.Contracts.MultiplayerGames;

public readonly record struct PlayerMadeMoveResponse(
    Guid PlayerId,
    Int2 Position,
    GameSymbol Symbol
);
