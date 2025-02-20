using h.Contracts.GameInvitations;
using h.Server.Infrastructure;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Features.GameInvitations;

public class GameInvitationHub : Hub<IGameInvitationHubClient>
{
    private readonly IHubUserIdMappingService<GameInvitationHub, MultiplayerGameUserIdentity> _userIdMappingService;
    public GameInvitationHub(IHubUserIdMappingService<GameInvitationHub, MultiplayerGameUserIdentity> userIdMappingService)
    {
        _userIdMappingService = userIdMappingService;
    }

    public override async Task OnConnectedAsync()
    {
        if(Context.User is not { Identity: { IsAuthenticated: true } })
        {
            Context.Abort();
            return;
        }

        // Add the connection ID to the mapping service
        var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);

        _userIdMappingService.Add(Context.ConnectionId, identity);

        await base.OnConnectedAsync();
    }

    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from mapping
        if(Context.User is {Identity: { IsAuthenticated: true } })
        {
            var identity = MultiplayerGameUserIdentity.FromNETIdentity(Context.User);
            _userIdMappingService.Remove(identity);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
