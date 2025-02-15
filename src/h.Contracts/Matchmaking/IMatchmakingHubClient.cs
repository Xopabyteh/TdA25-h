namespace h.Contracts.Matchmaking;
public interface IMatchmakingHubClient
{
    public const string Route = "matchmaking-hub";

    /// <summary>
    /// Invoked from the server when a match is found
    /// </summary>
    public Task MatchFound(FoundMatchingDetailsResponse foundMatching);
}
