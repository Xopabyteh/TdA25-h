namespace h.Server.Infrastructure.Matchmaking;

/// <summary>
/// Creating of matchings, accepting and rejecting
/// </summary>
public class InMemoryMatchmakingService
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<Guid, PlayerMatching> _playerMatchings = new(30);
    private readonly SortedList<DateTimeOffset, PlayerMatching> _playerMatchingsByCreationTime = new(30);

    public InMemoryMatchmakingService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Create a new player matching, which can be accepted or rejected
    /// by players.
    /// </summary>
    public PlayerMatching RegisterNewPlayerMatching(Guid player1, Guid player2)
    {
        lock(_playerMatchings)
        {
            var matching = new PlayerMatching(
                _timeProvider.GetUtcNow(),
                Guid.NewGuid(),
                player1,
                player2
            );

            _playerMatchings.Add(matching.Id, matching);
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
            if (_playerMatchings.TryGetValue(matchingId, out var matching))
            {
                _playerMatchings.Remove(matchingId);
                _playerMatchingsByCreationTime.Remove(matching.CreatedAt);
            }
        }
    }

    /// <summary>
    /// Unregister all expired player matchings.
    /// </summary>
    public void RemoveExpiredMatchings()
    {
        lock (_playerMatchings)
        {
            var timeOfExpiration = _timeProvider.GetUtcNow().AddSeconds(-PlayerMatching.WaitingMatchingTimeoutSeconds);

            while (
                _playerMatchingsByCreationTime.Count > 0
                && _playerMatchingsByCreationTime.Keys[0] < timeOfExpiration)
            {
                var matching = _playerMatchingsByCreationTime.Values[0];
                _playerMatchingsByCreationTime.RemoveAt(0);
                _playerMatchings.Remove(matching.Id);
            }
        }
    }
}
