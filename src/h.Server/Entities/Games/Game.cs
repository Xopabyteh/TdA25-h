using ErrorOr;
using h.Primitives.Games;
using System.Reflection;

namespace h.Server.Entities.Games;

public class Game
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public string Name { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public GameState GameState { get; set; }
    public GameBoard Board { get; set; }

    /// <summary>
    /// Creates a new game
    /// </summary>
    /// <param name="boardMatrix">
    /// [height][width], value is either '', 'X' or 'O'
    /// </param>
    private Game(string name, GameDifficulty difficulty, GameBoard board)
    {
        Name = name;
        Difficulty = difficulty;
        Board = board;

        GameState = GetGameState();
    }

    public static ErrorOr<Game> CreateNewGame(string name, GameDifficulty difficulty, GameBoard board)
    {
        // Validate board
        var boardValidationErrors = ValidateBoard(board);
        if (boardValidationErrors.Count > 0)
        {
            return ErrorOr<Game>.From(boardValidationErrors);
        }

        // Create game
        var game = new Game(name, difficulty, board);

        return game;
    }

    /// <summary>
    /// Validates the board to make sure it adheres to domain logic
    /// - There is balanced amount of X and O (equal or O has one more)
    /// - X started (implied by previous rule)
    /// </summary>
    private static List<Error> ValidateBoard(GameBoard board)
    {
        var resultErrors = new List<Error>();

        // Get counts of symbols
        var xoCount = board.GetSymbolCounts();

        var symbolAmDif = Math.Abs(xoCount.XsCount - xoCount.OsCount);
        if (symbolAmDif > 1)
        {
            resultErrors.Add(UnbalancedSymbolAmountError());
        }

        if(xoCount.OsCount > xoCount.XsCount)
        {
            // If there are more Os than Xs, the game must have started with O, instead of X
            resultErrors.Add(IncorrectStartingSymbolError());
        }

        return resultErrors;
    }

    private static Error UnbalancedSymbolAmountError()
        => Error.Validation(nameof(UnbalancedSymbolAmountError), "The amount of X and O symbols is not balanced. The amounts must be same, or one off maximum");

    private static Error IncorrectStartingSymbolError()
        => Error.Validation(nameof(IncorrectStartingSymbolError), "The game must start with X symbol");

    /// <summary>
    /// Determine game state based on the board state
    /// Zahájení - hra s 5 a méně koly
    /// Middle game - stav hry od 6. kola
    /// Koncovka - hráč má šanci svým dalším tahem propojit 5 v řadě a vyhrát hru
    /// </summary>
    private GameState GetGameState()
    {
        // A round is when both players have played
        // Remember, that cross always starts

        // Consider a valid state:
        // X started
        // There is balanced amount of X and O (equal or O has one more)
    
        return GameState.Unknown;
    }



    // Used by EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Game()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }
}
