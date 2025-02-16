using Carter;
using h.Contracts.Matchmaking;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Features.Matchmaking;

/// <summary>
/// Feature module for accepting or declining a matchmaking match
/// </summary>
public static class MatchmakingMatchingAcceptanceFeatureModule
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/accept/{matchingId:guid}", HandleAccept)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);
        
            app.MapPost("/api/v1/matchmaking/decline/{matchingId:guid}", HandleDecline)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);
        }
    }

    public static async Task<IResult> HandleAccept(
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

    public static async Task<IResult> HandleDecline(
        [FromServices] InMemoryMatchmakingService matchmakingService,
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
            async _ =>
            {
                // Notify players about the cancellation
                var connectionIds = matching.Value.GetPlayersInMatch()
                    .Select(userId => hubUserIdMappingService.GetConnectionId(userId)
                        ?? throw IHubUserIdMappingService<MatchmakingHub>.UserNotPresentException(userId));

                await hubContext.Clients.Clients(connectionIds).MatchCancelled();

                return Results.Ok();
            },
            error => Task.FromResult(ErrorResults.FromFirstError(error))
        );
    }
}
