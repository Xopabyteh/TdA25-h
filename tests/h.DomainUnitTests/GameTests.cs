using h.Primitives.Games;
using h.Server.Entities.Games;

namespace h.DomainUnitTests;
public class GameTests
{
    [Fact]
    public void CreateNewGame_WhereXAndOAreBalanced_ReturnsGame()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.BoardMatrix[0][0] = GameSymbol.O;
        board.BoardMatrix[1][0] = GameSymbol.O;
        board.BoardMatrix[2][0] = GameSymbol.X;
        board.BoardMatrix[3][0] = GameSymbol.X;

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.False(game.IsError);
    }

    [Fact]
    public void CreateNewGame_WhereXDidntStart_ReturnsError()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.BoardMatrix[0][0] = GameSymbol.O;
        board.BoardMatrix[1][0] = GameSymbol.O;
        board.BoardMatrix[2][0] = GameSymbol.X;

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.True(game.IsError);
    }


    [Fact]
    public void CreateNewGame_WhereTooManyXs_ReturnsError()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.BoardMatrix[0][0] = GameSymbol.X;
        board.BoardMatrix[1][0] = GameSymbol.X;

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.True(game.IsError);
    }

    [Fact]
    public void CreateNewGame_WhereTooManyOs_ReturnsError()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.BoardMatrix[0][0] = GameSymbol.O;
        board.BoardMatrix[1][0] = GameSymbol.O;

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.True(game.IsError);
    }
}
