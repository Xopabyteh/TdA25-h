namespace h.Contracts.GameInvitations;
public interface IGameInvitationHubClient
{
    public const string Route = "hub/game-invitation";
    
    /// <summary>
    /// Similar to <see cref="h.Contracts.Matchmaking.IMatchmakingHubClient.NewGameSessionCreated(Guid)"/>
    /// </summary>
    public Task NewGameSessionCreated(Guid gameId);
}
