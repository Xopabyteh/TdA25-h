using Carter;
using h.Contracts;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Matchmaking;

public static class JoinMatchmakingQueue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/join", HandleJoin)
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
        var queueResult = matchmakingQueue.AddUserToQueue(userId);

        return queueResult.MatchFirst(
            position => Results.Ok(position),
            ErrorResults.FromFirstError
        );
    }
}
