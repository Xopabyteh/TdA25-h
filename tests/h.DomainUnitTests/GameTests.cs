using h.Primitives.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure;

namespace h.DomainUnitTests;
public class GameTests
{
    [Fact]
    public void CreateNewGame_WhereXAndOAreBalanced_ReturnsGame()
    {
        // Arrange
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 3), GameSymbol.X);

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
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 2), GameSymbol.X);

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
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.X);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.X);

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
        board.SetSymbolAt(new Int2(0, 0), GameSymbol.O);
        board.SetSymbolAt(new Int2(0, 1), GameSymbol.O);

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.True(game.IsError);
    }

    [Fact]
    public void CreateNewGame_GameState_Opening_ClassifiedCorrectly()
    {
        // Arrange (4x and 4o)
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0,0), GameSymbol.X);
        board.SetSymbolAt(new Int2(1,0), GameSymbol.O);
        board.SetSymbolAt(new Int2(2,0), GameSymbol.X);
        board.SetSymbolAt(new Int2(3,0), GameSymbol.O);

        board.SetSymbolAt(new Int2(0,1), GameSymbol.O);
        board.SetSymbolAt(new Int2(1,1), GameSymbol.X);
        board.SetSymbolAt(new Int2(2,1), GameSymbol.O);
        board.SetSymbolAt(new Int2(3,1), GameSymbol.X);

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.False(game.IsError);
        Assert.Equal(GameState.Opening, game.Value.GameState);
    }

    [Fact]
    public void CreateNewGame_GameState_Midgame_ClassifiedCorrectly()
    {
        // Arrange (6x and 6o)
        var board = GameBoard.CreateNew();
        board.SetSymbolAt(new Int2(0,0), GameSymbol.X);
        board.SetSymbolAt(new Int2(1,0), GameSymbol.O);
        board.SetSymbolAt(new Int2(2,0), GameSymbol.X);
        board.SetSymbolAt(new Int2(3,0), GameSymbol.O);
        board.SetSymbolAt(new Int2(4,0), GameSymbol.X);
        board.SetSymbolAt(new Int2(5,0), GameSymbol.O);

        board.SetSymbolAt(new Int2(0,1), GameSymbol.X);
        board.SetSymbolAt(new Int2(1,1), GameSymbol.O);
        board.SetSymbolAt(new Int2(2,1), GameSymbol.X);
        board.SetSymbolAt(new Int2(3,1), GameSymbol.O);
        board.SetSymbolAt(new Int2(4,1), GameSymbol.X);
        board.SetSymbolAt(new Int2(6,1), GameSymbol.O);

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board);

        // Assert
        Assert.False(game.IsError);
        Assert.Equal(GameState.Midgame, game.Value.GameState);
    }

    [Fact]
    public void CreateNewGame_GameState_EndGame_ForX_ClassifiedCorrectly()
    {
        // Arrange
        var board = GameBoard.Parse(BoardXWinNextTurn);

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board.Value);

        // Assert
        Assert.False(board.IsError);
        Assert.False(game.IsError);

        Assert.Equal(GameState.Endgame, game.Value.GameState);
    }

    [Fact]
    public void CreateNewGame_GameState_EndGame_ForO_ClassifiedCorrectly()
    {
        // Arrange
        var board = GameBoard.Parse(BoardOWinNextTurn);

        // Act
        var game = Game.CreateNewGame("name", GameDifficulty.Easy, board.Value);

        // Assert
        Assert.False(board.IsError);
        Assert.False(game.IsError);

        Assert.Equal(GameState.Endgame, game.Value.GameState);
    }


    private static readonly string[][] BoardXWinNextTurn = [
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "O", "O", "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "X", "O", "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "X", "O", "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "X", "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "X",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",   "",  "",  "",  "",  "",  ""]
    ];

    private static readonly string[][] BoardOWinNextTurn = [
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "O", "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "O", "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "X", "O", "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "X", "O", "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "X", "X", "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "X", "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""],
        ["",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  "",  ""]
    ];
}