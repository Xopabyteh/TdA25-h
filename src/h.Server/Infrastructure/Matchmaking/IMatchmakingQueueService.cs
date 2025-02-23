using ErrorOr;

namespace h.Server.Infrastructure.Matchmaking;

/// <summary>
/// Works with matchmaking queue
/// </summary>
public interface IMatchmakingQueueService
{
    /// <summary>
    /// Adds user to the end of the queue,
    /// so he can be matched with other users
    /// </summary>
    /// <returns>Error or users place in queue</returns>
    public ErrorOr<int> AddUserToQueue(Guid userId);
    public bool RemoveUserFromQueue(Guid userId);
    /// <summary>
    /// Adds user to the front of the queue
    /// </summary>
    /// <returns>Error or users place in queue</returns>
    public ErrorOr<int> AddUserToStartOfQueue(Guid userId);
    public (Guid user1Id, Guid user2Id)? MatchUsers();
  
    /// <summary>
    /// To simplify testing
    /// </summary>
    internal Guid? PeekFirstInQueue();
    /// <summary>
    /// To simplify testing.
    /// </summary>
    /// <param name="tryTakeRange">
    /// How many users to try to take from the queue.
    /// If not enough, result will take until end of queue
    /// </param>
    internal ReadOnlySpan<Guid> PeekQueue(int tryTakeRange);

    /// <summary>
    /// How many players are in queue right now
    /// </summary>
    public int GetQueueSize();

    public int GetPositionInQueue(Guid userId);
}
