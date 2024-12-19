using h.Server.Entities.Games;
using h.Server.Infrastructure;

namespace h.DomainUnitTests;
public class BoardTests
{
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
}
