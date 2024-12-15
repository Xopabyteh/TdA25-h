using h.Primitives.Games;

namespace h.Contracts.Games;
public readonly record struct CreateNewGameRequest(string Name, GameDifficulty Difficulty, string[][] Board);
