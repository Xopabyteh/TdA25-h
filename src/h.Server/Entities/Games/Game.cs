using h.Primitives.Games;

namespace h.Server.Entities.Games;

public class Game
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public string Name { get; set; }
    public GameDifficulty Difficulty { get; init; }
    public GameState GameState { get; set; } // Todo: use
    public GameBoard Board { get; init; }

    /// <summary>
    /// Creates a new game
    /// </summary>
    /// <param name="boardMatrix">
    /// [height][width], value is either '', 'X' or 'O'
    /// </param>
    public Game(string name, GameDifficulty difficulty, GameBoard board)
    {
        Name = name;
        Difficulty = difficulty;
        Board = board;

        GameState = GameState.Unknown;
        
        // Todo:
        //DETAIL: Failing row contains (0193c1ca-20a0-7977-a104-68fe8f343a7c, -infinity, -infinity, Moje první hra, 3, null, [[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]...).
    }

    // Used by EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Game()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }
}
