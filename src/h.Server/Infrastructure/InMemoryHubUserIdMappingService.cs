
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace h.Server.Infrastructure;

public class InMemoryHubUserIdMappingService<THub>
    : IHubUserIdMappingService<THub>
    where THub : Hub
{
    private readonly ConcurrentDictionary<Guid, string> _userIdToConnectionId
        = new(concurrencyLevel: -1, capacity: 30);

    public void Add(string connectionId, Guid userId)
    {
        _userIdToConnectionId[userId] = connectionId;
    }

    public string? GetConnectionId(Guid userId)
    {
        return _userIdToConnectionId.TryGetValue(userId, out var connectionId)
            ? connectionId
            : null;
    }

    public void Remove(Guid userId)
    {
        _userIdToConnectionId.TryRemove(userId, out _);
    }
}
