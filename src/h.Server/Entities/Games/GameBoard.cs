namespace h.Server.Entities.Games;

public class GameBoard
{
    public const int PREDEFINED_BOARD_SIDE_SIZE = 15;
    /// <summary>
    /// [height][width]
    /// </summary>
    public GameSymbol[][] BoardMatrix { get; init; }

    private GameBoard(int width, int height)
    {
        BoardMatrix = new GameSymbol[height][];
        for (int i = 0; i < width; i++)
        {
            BoardMatrix[i] = new GameSymbol[width];
        }
    }

    // Used by EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private GameBoard()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public static GameBoard CreateNew()
    {
        return new(PREDEFINED_BOARD_SIDE_SIZE, PREDEFINED_BOARD_SIDE_SIZE);
    }
}