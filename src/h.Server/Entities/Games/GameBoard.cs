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
    public static GameBoard Parse(string[][] boardMatrix)
    {
        var gameBoard = CreateNew();

        // Todo: ensure board matrix size is validated elsewhere
        if (boardMatrix.Length != PREDEFINED_BOARD_SIDE_SIZE)
            throw new IncorrectBoardSizeException();

        for (int y = 0; y < boardMatrix.Length; y++)
        {
            var row = boardMatrix[y];
            if (row.Length != PREDEFINED_BOARD_SIDE_SIZE)
                throw new IncorrectBoardSizeException();

            for (int x = 0; x < row.Length; x++)
            {
                var cell = row[x];
                if (cell.Length > 1)
                    throw new IncorrectCellSizeException();

                if(cell.Length == 0)
                {
                    // -> No symbol
                    gameBoard.BoardMatrix[y][x] = GameSymbol.None;
                    continue;
                }

                // -> Some symbol
                gameBoard.BoardMatrix[y][x] = GameSymbolParser.Parse(cell[0]);
            }
        }

        return gameBoard;
    }

    public class IncorrectBoardSizeException : Exception
    {
        public IncorrectBoardSizeException() : base("Board matrix must be 15x15")
        {
        }
    }

    public class IncorrectCellSizeException : Exception
    {
        public IncorrectCellSizeException() : base("Cell must be empty or a single character")
        {
        }
    }
}