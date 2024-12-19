using ErrorOr;
using System;

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

    // Todo: consider changing to Span2D<>
    public static ErrorOr<GameBoard> Parse(string[][] boardMatrix)
    {
        var gameBoard = CreateNew();

        // Todo: ensure board matrix size is validated elsewhere
        if (boardMatrix.Length != PREDEFINED_BOARD_SIDE_SIZE)
            return IncorrectBoardSizeError();

        for (int y = 0; y < boardMatrix.Length; y++)
        {
            var row = boardMatrix[y];
            if (row.Length != PREDEFINED_BOARD_SIDE_SIZE)
                return IncorrectBoardSizeError();

            for (int x = 0; x < row.Length; x++)
            {
                var cell = row[x];
                if (cell.Length > 1)
                    return IncorrectCellFormatError(x, y);

                if (cell.Length == 0)
                {
                    // -> No symbol
                    gameBoard.BoardMatrix[y][x] = GameSymbol.None;
                    continue;
                }

                // -> Some symbol
                var symbol = GameSymbolParser.Parse(cell[0]);
                if(symbol == GameSymbol.None)
                    // We were supposed to have a symbol here, but parsing failed
                    return IncorrectCellFormatError(x, y); 

                gameBoard.BoardMatrix[y][x] = symbol;
            }
        }

        return gameBoard;
    }
    
    public static string[][] BoardMatrixToString(GameSymbol[][] boardMatrix)
    {
        var result = new string[PREDEFINED_BOARD_SIDE_SIZE][];
        for (int y = 0; y < boardMatrix.Length; y++)
        {
            var row = boardMatrix[y];
            result[y] = new string[PREDEFINED_BOARD_SIDE_SIZE];
            for (int x = 0; x < row.Length; x++)
            {
                var cell = row[x];
                result[y][x] = cell switch
                {
                    GameSymbol.None => "",
                    GameSymbol.X => "X",
                    GameSymbol.O => "O",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        return result;
    }

    /// <returns>How many xs and os are on the board. Complexity: O(w*h)</returns>
    public (int XsCount, int OsCount) GetSymbolCounts()
    {
        var xoCount = BoardMatrix
            .SelectMany(rows => rows)
            .Aggregate(
                (XsCount: 0, OsCount: 0),
                (accumulate, symbol) =>
                    symbol switch
                    {
                        GameSymbol.X => (accumulate.XsCount + 1, accumulate.OsCount),
                        GameSymbol.O => (accumulate.XsCount, accumulate.OsCount + 1),
                        _ => accumulate
                });

        return xoCount;
    }

    public static Error IncorrectBoardSizeError()
        => Error.Validation(nameof(IncorrectBoardSizeError) ,"Board size does not match specification");

    public static Error IncorrectCellFormatError(int x, int y)
        => Error.Validation(nameof(IncorrectCellFormatError), $"The cell {x},{y} does not match the specification");
}