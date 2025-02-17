﻿using ErrorOr;
using h.Contracts;

namespace h.Server.Infrastructure.Matchmaking;

public class InMemoryMatchmakingQueueService : IMatchmakingQueueService
{
    private readonly List<Guid> _queue = new(30);

    private readonly ILogger<InMemoryMatchmakingQueueService> _logger;

    public InMemoryMatchmakingQueueService(ILogger<InMemoryMatchmakingQueueService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds the user to the back of the queue
    /// </summary>
    /// <returns>Position of user in queue (indexed from 0)</returns>
    public ErrorOr<int> AddUserToQueue(Guid userId)
    {
        lock (_queue) {
            if (_queue.Contains(userId))
                return SharedErrors.Matchmaking.UserAlreadyInQueue();

            _queue.Add(userId);
        }
        _logger.LogInformation("User {UserId} added to queue", userId);
        return _queue.Count - 1;
    }

    /// <summary>
    /// Removes the user from the queue
    /// (i. e. the user left the queue)
    /// </summary>
    /// <returns>True if the user was removed successfully</returns>
    public bool RemoveUserFromQueue(Guid userId)
    {
        _logger.LogInformation("Removing {userId} from the queue", userId);
        lock (_queue) {
            return _queue.Remove(userId);
        }
    }

    /// <summary>
    /// Adds the user to the front of the queue
    /// (i. e. the other player declined the match, so the user is requeued)
    /// </summary>
    /// <returns>Position of user in queue (indexed from 0)</returns>
    public ErrorOr<int> AddUserToStartOfQueue(Guid userId)
    {
        lock (_queue) {
            if (_queue.Contains(userId))
                return SharedErrors.Matchmaking.UserAlreadyInQueue();

            _queue.Insert(0, userId);
        }

        _logger.LogInformation("User {UserId} added to start of queue", userId);
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

    Guid? IMatchmakingQueueService.GetFirstInQueue()
    {
        lock(_queue)
        {
            return _queue.Count > 0
                ? _queue[0]
                : null;
        }
    }
}