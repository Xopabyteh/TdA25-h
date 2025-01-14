using h.Primitives.Games;

namespace h.Contracts.Games;
public readonly record struct UpdateGameRequest(string Name, GameDifficulty Difficulty, string[][] Board, Guid GameId);
