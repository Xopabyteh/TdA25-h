
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace h.Server.Infrastructure;

/// <summary>
/// Only one type provided -> use guid
/// </summary>
public class InMemoryHubUserIdMappingService<THub>
    : InMemoryHubUserIdMappingService<THub, Guid>, IHubUserIdMappingService<THub>
    where THub : Hub
{
}

/// <summary>
/// In memory mapping from SignalR connection to an identity
/// </summary>
public class InMemoryHubUserIdMappingService<THub, TIdentity>
    : IHubUserIdMappingService<THub, TIdentity>
    where THub : Hub
    where TIdentity : notnull
{
    private readonly ConcurrentDictionary<TIdentity, string> _userIdToConnectionId
        = new(concurrencyLevel: -1, capacity: 30);

    public void Add(string connectionId, TIdentity userId)
    {
        _userIdToConnectionId[userId] = connectionId;
    }

    public string? GetConnectionId(TIdentity userId)
    {
        return _userIdToConnectionId.TryGetValue(userId, out var connectionId)
            ? connectionId
            : null;
    }

    public void Remove(TIdentity userId)
    {
        _userIdToConnectionId.TryRemove(userId, out _);
    }
}
