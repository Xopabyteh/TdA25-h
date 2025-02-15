namespace h.Server.Infrastructure.Matchmaking;

public class InMemoryMatchmakingQueueService : IMatchmakingQueueService
{
    private readonly List<Guid> _queue = new(30);
    /// <summary>
    /// Adds the user to the back of the queue
    /// </summary>
    /// <returns>Position of user in queue (indexed from 0)</returns>
    public int AddUserToQueue(Guid userId)
    {
        lock (_queue) {
            _queue.Add(userId);
        }
        return _queue.Count - 1;
    }

    /// <summary>
    /// Removes the user from the queue
    /// (i. e. the user left the queue)
    /// </summary>
    /// <returns>True if the user was removed successfully</returns>
    public bool RemoveUserFromQueue(Guid userId)
    {
        lock (_queue) {
            return _queue.Remove(userId);
        }
    }

    /// <summary>
    /// Adds the user to the front of the queue
    /// (i. e. the other player declined the match, so the user is requeued)
    /// </summary>
    /// <returns>Position of user in queue (indexed from 0)</returns>
    public int AddUserToStartOfQueue(Guid userId)
    {
        lock (_queue) {
            _queue.Insert(0, userId);
        }

        return 0;
    }

    /// <summary>
    /// Removes the first two users in queue and returns them.
    /// Returns null if a match cannot be made.
    /// </summary>
    public (Guid user1Id, Guid user2Id)? MatchUsers()
    {
        lock(_queue) {
            if (_queue.Count < 2)
                return null;

            var user1Id = _queue[0];
            var user2Id = _queue[1];
        
            _queue.RemoveRange(0, 2);
            return (user1Id, user2Id);
        }
    }
}