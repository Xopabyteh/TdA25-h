﻿using h.Contracts.Matchmaking;
using h.Server.Infrastructure.Auth;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Infrastructure.Matchmaking;

public class MatchmakingHub : Hub<IMatchmakingHubClient>
{
    private readonly IHubUserIdMappingService<MatchmakingHub> _userIdMappingService;

    public MatchmakingHub(IHubUserIdMappingService<MatchmakingHub> userIdMappingService)
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
        var userId = Context.User.GetUserId();
        _userIdMappingService.Add(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from mapping
        if(Context.User is {Identity: { IsAuthenticated: true } })
        {
            var userId = Context.User.GetUserId();
            _userIdMappingService.Remove(userId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
