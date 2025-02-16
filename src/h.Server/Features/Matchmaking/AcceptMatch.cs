using h.Contracts.Matchmaking;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Carter;

namespace h.Server.Features.Matchmaking;

public static class AcceptMatch
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/accept/{matchingId:guid}", Handle)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] InMemoryMatchmakingService matchmakingService,
        [FromServices] IHubUserIdMappingService<MatchmakingHub> hubUserIdMappingService,
        [FromServices] IHubContext<MatchmakingHub, IMatchmakingHubClient> hubContext,
        [FromRoute] Guid matchingId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var result = matchmakingService.AcceptMatching(matchingId, userId);

        return await result.MatchFirst(
            async remainingAccepteesCount =>
            {
                // Notify players about the acceptance
                var matching = matchmakingService.GetMatching(matchingId)!;
                
                // Todo: handle potential of a player leaving mid-acceptance
                var connectionIds = matching.Value.GetPlayersInMatch()
                    .Select(userId => hubUserIdMappingService.GetConnectionId(userId)
                        ?? throw IHubUserIdMappingService<MatchmakingHub>.UserNotPresentException(userId));

                await hubContext.Clients.Clients(connectionIds).PlayerAccepted(userId);

                if (remainingAccepteesCount == 0)
                {
                    // Todo:
                    // Start match and notify players
                }

                return Results.Ok();
            },
            error => Task.FromResult(ErrorResults.FromFirstError(error))
        );
    }   
}
