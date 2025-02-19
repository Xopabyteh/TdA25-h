using Microsoft.AspNetCore.SignalR;

namespace h.Server.Infrastructure;


/// <summary>
/// A service that maps SignalR hub connections to user IDs
/// </summary>
public interface IHubUserIdMappingService<THub> 
    : IHubUserIdMappingService<THub, Guid>
    where THub : Hub
{ }

/// <summary>
/// A service that maps SignalR hub connections to user IDs
/// </summary>
public interface IHubUserIdMappingService<THub, TIdentity> 
    where THub : Hub
    where TIdentity : notnull
{
    public void Add(string connectionId, TIdentity userId);
    public void Remove(TIdentity userId);
    public string? GetConnectionId(TIdentity userId);

    public class UserNotPresentInMappingException : Exception
    {
        public UserNotPresentInMappingException(TIdentity userId)
            : base($"User with ID {userId} is not present in the mapping")
        {
        }
    }

    public static UserNotPresentInMappingException UserNotPresentException(TIdentity userId)
        => new(userId);
}

