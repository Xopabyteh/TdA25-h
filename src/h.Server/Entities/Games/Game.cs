using ErrorOr;
using h.Primitives.Games;
using h.Server.Infrastructure;

namespace h.Server.Entities.Games;

public class Game
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
    public string Name { get; private set; }
    public GameDifficulty Difficulty { get; private set; }
    public GameState GameState { get; private set; }
    public GameBoard Board { get; private set; }

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

    /// <summary>
    /// Constructs a new game object with validation
    /// </summary>
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
    /// Updates the game with new values
    /// and recalculates the game state
    /// </summary>
    public void Update(string name, GameDifficulty difficulty, GameBoard board)
    {
        Name = name;
        Difficulty = difficulty;
        Board = board;

        GameState = GetGameState();
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
        // - The method considers a valid game state

        var symbolCounts = Board.GetSymbolCounts();

        // Get am of rounds (both players played)
        // X -> 0
        // XO -> 1
        // XOX -> 1
        // XOXO -> 2
        var amOfRounds = (symbolCounts.XsCount + symbolCounts.OsCount) / 2;

        // Remember, cross always starts
        var symbolInPlay = symbolCounts.XsCount > symbolCounts.OsCount
            ? GameSymbol.O
            : GameSymbol.X;

        // Check if the game can be ended directly by the symbol in play (koncovka)
        // (there are four in a row of the symbol in play)
        for (int y = 0; y < GameBoard.PREDEFINED_BOARD_SIDE_SIZE; y++)
        for (int x = 0; x < GameBoard.PREDEFINED_BOARD_SIDE_SIZE; x++)
        {
            var checkPos = new Int2(x, y);

            if (Board.GetSymbolAt(checkPos) != symbolInPlay)
                continue;

            // Check all directions
            foreach (var dir in Int2.OrthoAndDiagonalDirections)
            {
                var count = Board.GetSymbolsInRowInDirection(checkPos, dir);
                if (count >= 4)
                {
                    return GameState.Endgame;
                }
            }
        }

        // -> Not endgame, determine by round count
        return amOfRounds switch
        {
            <= 5 => GameState.Opening,
            > 5 => GameState.Midgame,
        };
    }

    // Used by EF Core
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Game()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }
}
