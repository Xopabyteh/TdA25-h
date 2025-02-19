using h.Primitives;
using h.Primitives.Games;
using h.Server.Entities.Games;

namespace h.DomainUnitTests;
public class BoardTests
{
    [Fact]
    public void Board_GetSymbolsInRowInDirection_ReturnsCorrectCount()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 3), GameSymbol.X);

        // Act
        var count = board.GetSymbolsInRowInDirection(new Int2(0, 0), new Int2(0, 1));

        // Assert
        Assert.Equal(4, count);
    }

    /// <summary>
    /// Check that elements in X direction are really in width direction
    /// and Y corresponds to height.
    /// </summary>
    [Fact]
    public void Board_ParsedBoard_XYOrientation_MatchesVisualOrientation()
    {
        // Arrange
        var boardResult = GameBoard.Parse([
            ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
            ["",  "",  "", "", "", "", "", "", "", "", "", "", "", "", ""],
        ]);

        // Assert
        Assert.False(boardResult.IsError);
        Assert.Equal(GameSymbol.O, boardResult.Value.GetSymbolAt(new Int2(0, 0)));
        Assert.Equal(GameSymbol.X, boardResult.Value.GetSymbolAt(new Int2(1, 0)));
        Assert.Equal(GameSymbol.None, boardResult.Value.GetSymbolAt(new Int2(2, 0)));
        Assert.Equal(GameSymbol.O, boardResult.Value.GetSymbolAt(new Int2(0, 1)));
        Assert.Equal(GameSymbol.X, boardResult.Value.GetSymbolAt(new Int2(1, 1)));
    }

    [Fact]
    public void Board_GetSymbolCounts_ReturnsCorrectCounts()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 3), GameSymbol.O);

        // Act
        var counts = board.GetSymbolCounts();

        // Assert
        Assert.Equal(2, counts.XsCount);
        Assert.Equal(2, counts.OsCount);
    }

    [Fact]
    public void Board_IsWinningSymbol_ReturnsTrueForWinningMove()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 3), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 4), GameSymbol.X);

        // Act
        var isWinning = board.IsWinningSymbol(new Int2(0, 4), GameSymbol.X);

        // Assert
        Assert.True(isWinning);
    }

    [Fact]
    public void Board_IsWinningSymbol_ReturnsFalseForNonWinningMove()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 3), GameSymbol.X);

        // Act
        var isWinning = board.IsWinningSymbol(new Int2(0, 3), GameSymbol.X);

        // Assert
        Assert.False(isWinning);
    }

    [Fact]
    public void Board_IsDraw_ReturnsTrueForFullBoard()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        for (int y = 0; y < board.BoardMatrix.Length; y++)
        {
            for (int x = 0; x < board.BoardMatrix[0].Length; x++)
            {
                board.SetSymbolAt(new Int2(y, x), (y + x) % 2 == 0 ? GameSymbol.X : GameSymbol.O);
            }
        }

        // Act
        var isDraw = board.IsDraw();

        // Assert
        Assert.True(isDraw);
    }

    [Fact]
    public void Board_IsDraw_ReturnsFalseForBoardWithEmptyCells()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.O);
        // Leave some empty spaces

        // Act
        var isDraw = board.IsDraw();

        // Assert
        Assert.False(isDraw);
    }
}
