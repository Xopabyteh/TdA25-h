namespace h.Contracts.Matchmaking;
public sealed class MatchmakingOptions
{
    public const string SectionName = "Matchmaking";

    public int PlayerHasToAcceptInSeconds { get; set; }
    public int MatchingExpiresInSeconds { get; set; }
}
