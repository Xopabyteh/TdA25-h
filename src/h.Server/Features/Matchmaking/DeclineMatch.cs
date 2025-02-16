using Carter;
using h.Contracts.Matchmaking;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Features.Matchmaking;

public static class DeclineMatch
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/decline/{matchingId:guid}", Handle)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);
        }
    }
    public static async Task<IResult> Handle(
        [FromServices] InMemoryMatchmakingService matchmakingService,
        [FromServices] IMatchmakingQueueService matchmakingQueueService,
        [FromServices] IHubUserIdMappingService<MatchmakingHub> hubUserIdMappingService,
        [FromServices] IHubContext<MatchmakingHub, IMatchmakingHubClient> hubContext,
        [FromRoute] Guid matchingId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var matching = matchmakingService.GetMatching(matchingId)!;
        var result = matchmakingService.DeclineAndRemoveMatching(matchingId, userId);

        return await result.MatchFirst(
            async acceptees =>
            {
                // Notify players about the cancellation
                var connectionIds = matching.Value.GetPlayersInMatch()
                    .Select(userId => hubUserIdMappingService.GetConnectionId(userId)
                        ?? throw IHubUserIdMappingService<MatchmakingHub>.UserNotPresentException(userId));

                // Add other player back to the queue
                var otherPlayer = matching.Value.GetPlayersInMatch().First(playerId => playerId != userId);
                matchmakingQueueService.AddUserToStartOfQueue(otherPlayer);

                await hubContext.Clients.Clients(connectionIds).MatchCancelled(matchingId);

                return Results.Ok();
            },
            error => Task.FromResult(ErrorResults.FromFirstError(error))
        );
    }
}
