namespace h.Server.Entities.Users;

public struct ThinkDifferentElo
{
    public const ulong INITIAL_ELO = 400;

    /// <summary>
    /// Elo should be rounded up
    /// </summary>
    public ulong Rating { get; set; } = INITIAL_ELO;

    public ThinkDifferentElo(ulong rating)
    {
        Rating = rating;
    }

    public ThinkDifferentElo EloAfterLoss(double wins, double draws, double losses, double opponentRating)
    {
        const double LOSS_SA = 0;
        return EloFromSA(Rating, LOSS_SA, wins, draws, losses, opponentRating);
    }

    public ThinkDifferentElo EloAfterDraw(double wins, double draws, double losses, double opponentRating)
    {
        const double DRAW_SA = 0.5;
        return EloFromSA(Rating, DRAW_SA, wins, draws, losses, opponentRating);
    }

    public ThinkDifferentElo EloAfterWin(double wins, double draws, double losses, double opponentRating)
    {
        const double WIN_SA = 1;
        return EloFromSA(Rating, WIN_SA, wins, draws, losses, opponentRating);
    }

    private static ThinkDifferentElo EloFromSA(double rating, double sA, double wins, double draws, double losses, double opponentRating)
    {
        var eA = ExpectedScoreForA(rating, opponentRating);
        var newRating = RatingAfterGameForA(rating, sA, eA, wins, draws, losses);

        return new ThinkDifferentElo(newRating);
    }

    /// <summary>
    /// Calculates the new rating for player A after a game.
    /// </summary>
    /// <param name="rA">rating of player A.</param>
    /// <param name="sA">skutečný výsledek (1 za vihru, 0,5 za remfzu, 0 za prohru).</param>
    /// <param name="eA"><see cref="ExpectedScoreForA(double, double)"/></param>
    private static ulong RatingAfterGameForA(double rA, double sA, double eA, double wins, double draws, double losses)
    {
        //R'_A = R_A + 40 * [(S_A - E_A) * (1 + 0.5 * (0.5 - (W + D) / (W + D + L)))].
        const double ALPHA = 0.5;
        const double K_FACTOR = 40;
        
        var wdlRatio = (wins + draws) / (wins + draws + losses);
        var eloDelta = K_FACTOR * ((sA - eA) * (1 + ALPHA*(ALPHA-wdlRatio)));

        var rating = rA + eloDelta;

        var ratingCeiled = Math.Ceiling(rating);
        return (ulong)ratingCeiled;
    }

    /// <summary>
    /// E_A = 1 / (1 + 10^((R_B - R_A) / 400))
    /// </summary>
    private static double ExpectedScoreForA(double ratingA, double ratingB)
    {
        const double SCALING_FACTOR = 400;
        return 1 / (1 + Math.Pow(10, (ratingB - ratingA) / SCALING_FACTOR));
    }
}
