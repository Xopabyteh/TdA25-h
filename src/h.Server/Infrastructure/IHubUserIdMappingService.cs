using Microsoft.AspNetCore.SignalR;

namespace h.Server.Infrastructure;


/// <summary>
/// A service that maps SignalR hub connections to user IDs
/// </summary>
public interface IHubUserIdMappingService<THub> 
    where THub : Hub
{
    public void Add(string connectionId, Guid userId);
    public void Remove(Guid userId);
    public string? GetConnectionId(Guid userId);

    public class UserNotPresentInMappingException : Exception
    {
        public UserNotPresentInMappingException(Guid userId)
            : base($"User with ID {userId} is not present in the mapping")
        {
        }
    }

    public static UserNotPresentInMappingException UserNotPresentException(Guid userId)
        => new(userId);
}
