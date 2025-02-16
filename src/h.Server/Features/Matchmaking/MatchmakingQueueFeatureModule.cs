using Carter;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Matchmaking;

/// <summary>
/// Module used for joining and leaving the queue.
/// </summary>
public static class MatchmakingQueueFeatureModule
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/join", HandleJoin)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);

            app.MapPost("/api/v1/matchmaking/leave", HandleLeave)
                .RequireAuthorization(AppPolicyNames.AbleToJoinMatchmaking);
        }
    }

    /// <summary>
    /// Adds the user to the matchmaking queue
    /// </summary>
    /// <returns>Position of user in queue (Indexing from 0)</returns>
    public static IResult HandleJoin(
        [FromServices] IMatchmakingQueueService matchmakingQueue,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Todo: make sure he is not already in the queue
        var userId = httpContext.User.GetUserId();
        var posInQueue = matchmakingQueue.AddUserToQueue(userId);
        
        return Results.Ok(posInQueue);
    }

    public static IResult HandleLeave(
        [FromServices] IMatchmakingQueueService matchmakingQueue,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var removed = matchmakingQueue.RemoveUserFromQueue(userId);

        return removed
            ? Results.Ok()
            : ErrorResults.NotFound();
    }
}