using ErrorOr;
using h.Contracts;

namespace h.Server.Infrastructure.Matchmaking;

/// <summary>
/// Creating of matchings, accepting and rejecting
/// </summary>
public class InMemoryMatchmakingService
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<Guid, PendingPlayerMatching> _playerMatchings = new(30);
    /// <summary>
    /// Key: PlayerMatching.Id
    /// Value: The players that accepted the matching.
    /// Size of the array is always 2.
    /// </summary>
    private readonly Dictionary<Guid, List<Guid>> _playerMatchingToAcceptees = new(30); 
    private readonly SortedList<DateTimeOffset, PendingPlayerMatching> _playerMatchingsByCreationTime = new(30);

    public InMemoryMatchmakingService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Create a new player matching, which can be accepted or rejected
    /// by players.
    /// </summary>
    public PendingPlayerMatching RegisterNewPlayerMatching(Guid player1, Guid player2)
    {
        lock(_playerMatchings)
        {
            var matching = new PendingPlayerMatching(
                _timeProvider.GetUtcNow(),
                Guid.NewGuid(),
                player1,
                player2
            );

            _playerMatchings.Add(matching.Id, matching);
            _playerMatchingToAcceptees.Add(matching.Id, new (2));
            _playerMatchingsByCreationTime.Add(matching.CreatedAt, matching);


            return matching;
        }
    }

    /// <summary>
    /// Unregister a player matching.
    /// </summary>
    public void RemovePlayerMatching(Guid matchingId)
    {
        lock (_playerMatchings)
        {
            RemovePlayerMatchingInternal(matchingId);
        }
    }

    /// <summary>
    /// Removes a matching without locking,
    /// so it can be shared between methods.
    /// </summary>
    /// <param name="matchingId"></param>
    private void RemovePlayerMatchingInternal(Guid matchingId)
    {
        if (_playerMatchings.TryGetValue(matchingId, out var matching))
        {
            _playerMatchings.Remove(matchingId);
            _playerMatchingToAcceptees.Remove(matchingId);
            _playerMatchingsByCreationTime.Remove(matching.CreatedAt);
        }
    }

    /// <summary>
    /// Unregister all expired player matchings.
    /// </summary>
    public void RemoveExpiredMatchings()
    {
        // Todo: use this method
        lock (_playerMatchings)
        {
            var timeOfExpiration = _timeProvider.GetUtcNow().AddSeconds(-PendingPlayerMatching.WaitingMatchingTimeoutSeconds);

            while (
                _playerMatchingsByCreationTime.Count > 0
                && _playerMatchingsByCreationTime.Keys[0] < timeOfExpiration)
            {
                var matching = _playerMatchingsByCreationTime.Values[0];
                RemovePlayerMatchingInternal(matching.Id);
            }
        }
    }

    /// <summary>
    /// Marks a matching as accepted by the user.
    /// </summary>
    /// <returns>
    /// The remaining amount of accepts required.
    /// 0 -> everyone accepted
    /// </returns>
    public ErrorOr<int> AcceptMatching(Guid matchingId, Guid userId)
    {
        lock (_playerMatchings)
        {
            if (!_playerMatchings.TryGetValue(matchingId, out var matching))
                return SharedErrors.Matchmaking.MatchingNotFound();

            if (matching.Player1Id != userId && matching.Player2Id != userId)
                return SharedErrors.Matchmaking.UserNotPartOfMatching();

            var acceptees = _playerMatchingToAcceptees[matchingId];
            
            if(acceptees.Contains(userId))
                return SharedErrors.Matchmaking.MatchingAlreadyAccepted();

            // Add the user to the list of acceptees
            acceptees.Add(userId);

            // Return
            return 2 - acceptees.Count;
        }
    }

    public ErrorOr<Unit> DeclineAndRemoveMatching(Guid matchingId, Guid userId)
    {
        lock (_playerMatchings)
        {
            if (!_playerMatchings.TryGetValue(matchingId, out var matching))
                return SharedErrors.Matchmaking.MatchingNotFound();

            if (matching.Player1Id != userId && matching.Player2Id != userId)
                return SharedErrors.Matchmaking.UserNotPartOfMatching();

            RemovePlayerMatchingInternal(matchingId);
            return Unit.Value;
        }
    }

    public PendingPlayerMatching? GetMatching(Guid matchingId)
    {
        lock (_playerMatchings)
        {
            var found = _playerMatchings.TryGetValue(matchingId, out var matching);
            return found
                ? matching
                : null;
        }
    }
}
