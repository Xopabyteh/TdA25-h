namespace h.Contracts.Matchmaking;
public interface IMatchmakingHubClient
{
    public const string Route = "hub/matchmaking-hub";

    /// <summary>
    /// Invoked from the server when a match is found
    /// </summary>
    public Task MatchFound(FoundMatchingDetailsResponse foundMatching);

    /// <summary>
    /// Invoked from server when a player accepts the match.
    /// This will also be invoked for the player that just accepted the match
    /// </summary>
    public Task PlayerAccepted(Guid playerId);

    /// <summary>
    /// Invoked from server when a match is cancelled:
    /// expired or one of the players rejected the match
    /// </summary>
    public Task MatchCancelled(MatchCancelledResponse response);

    /// <summary>
    /// After everyone accepts, a new game session is created
    /// </summary>
    public Task NewGameSessionCreated(Guid gameId);
}
