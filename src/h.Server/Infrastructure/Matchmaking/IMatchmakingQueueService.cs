namespace h.Server.Infrastructure.Matchmaking;

/// <summary>
/// Works with matchmaking queue
/// </summary>
public interface IMatchmakingQueueService
{
    public int AddUserToQueue(Guid userId);
    public bool RemoveUserFromQueue(Guid userId);
    public int AddUserToStartOfQueue(Guid userId);
    public (Guid user1Id, Guid user2Id)? MatchUsers();
    /// <summary>
    /// To simplify testing
    /// </summary>
    internal Guid? GetFirstInQueue();
}
