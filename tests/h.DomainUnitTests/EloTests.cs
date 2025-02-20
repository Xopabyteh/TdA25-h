using h.Server.Entities.Users;

namespace h.DomainUnitTests;
public class EloTests
{
    [Fact]
    public void EloAfterWin_ShouldIncreaseRating()
    {
        // Arrange
        var initialRating = ThinkDifferentElo.INITIAL_ELO;
        var elo = new ThinkDifferentElo(initialRating);
        var opponentRating = 500.0;

        // Act
        var newElo = elo.EloAfterWin(wins: 10, draws: 5, losses: 5, opponentRating);

        // Assert
        Assert.True(newElo.Rating > initialRating);
    }

    [Fact]
    public void EloAfterDraw_ShouldChangeRating()
    {
        // Arrange
        var initialRating = ThinkDifferentElo.INITIAL_ELO;
        var elo = new ThinkDifferentElo(initialRating);
        var opponentRating = 500.0;

        // Act
        var newElo = elo.EloAfterDraw(wins: 10, draws: 5, losses: 5, opponentRating);

        // Assert
        Assert.NotEqual(initialRating, newElo.Rating);
    }

    [Fact]
    public void EloAfterLoss_ShouldDecreaseRating()
    {
        // Arrange
        var initialRating = ThinkDifferentElo.INITIAL_ELO;
        var elo = new ThinkDifferentElo(initialRating);
        var opponentRating = 500.0;

        // Act
        var newElo = elo.EloAfterLoss(wins: 10, draws: 5, losses: 5, opponentRating);

        // Assert
        Assert.True(newElo.Rating < initialRating);
    }
}
