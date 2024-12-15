using h.Primitives.Games;

namespace h.Contracts.Games;
public readonly record struct GameResponse(
    Guid Uuid,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Name,
    GameDifficulty Difficulty,
    GameState GameState,
    string[][] Board);
