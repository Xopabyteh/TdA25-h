namespace h.Server.Entities.Games;

public class Game
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public string Name { get; set; }
    public GameDifficulty Difficulty { get; init; }
    public GameState GameState { get; set; }
    public GameBoard Board { get; init; }
}
