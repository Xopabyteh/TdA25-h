namespace h.Server.Infrastructure.Matchmaking;

/// <summary>
/// Works with matchmaking queue
/// </summary>
public interface IMatchmakingQueueService
{
    int AddUserToQueue(Guid userId);
    bool RemoveUserFromQueue(Guid userId);
    int AddUserToStartOfQueue(Guid userId);
    (Guid user1Id, Guid user2Id)? MatchUsers();
}
