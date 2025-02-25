using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Matchmaking;

public static class JoinMatchmakingQueue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/join", HandleJoin)
                .RequireAuthorization(nameof(AppPolicies.AbleToJoinMatchmaking));
        }
    }

    /// <summary>
    /// Adds the user to the matchmaking queue.
    /// Ensure user is in hub
    /// </summary>
    /// <returns>Position of user in queue (Indexing from 0)</returns>
    public static async Task<IResult> HandleJoin(
        [FromServices] IMatchmakingQueueService matchmakingQueue,
        [FromServices] IHubUserIdMappingService<MatchmakingHub> hubMapping,
        [FromServices] AppDbContext db,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId()!.Value;

        // Make sure user isnt banned
        // (even though we require authorization policy, if user was banned while logged in,
        // his auth token would still be valid)
        var isBanned = await db.UsersDbSet
            .Where(u => u.Uuid == userId)
            .Select(u => u.BannedFromRankedMatchmakingAt)
            .FirstOrDefaultAsync(cancellationToken) is not null;
        
        if(isBanned)
            return Results.Forbid();

        var connectionId = hubMapping.GetConnectionId(userId);
        if (connectionId is null)
            return ErrorResults.Conflict($"You must be connected to {nameof(MatchmakingHub)}");

        var queueResult = matchmakingQueue.AddUserToQueue(userId);

        return queueResult.MatchFirst(
            position => Results.Ok(position),
            ErrorResults.FromFirstError
        );
    }
}
