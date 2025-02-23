using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Matchmaking;

public static class LeaveMatchmakingQueue
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/leave", HandleLeave)
                .RequireAuthorization(nameof(AppPolicies.AbleToJoinMatchmaking));
        }
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
