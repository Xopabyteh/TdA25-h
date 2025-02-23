using Carter;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Matchmaking;

public static class GetQueueSize
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/matchmaking/queue-size", HandleGetQueueSize);
        }
    }

    /// <summary>
    /// Gets the current size of the matchmaking queue.
    /// </summary>
    /// <returns>The size of the queue</returns>
    public static IResult HandleGetQueueSize(
        [FromServices] IMatchmakingQueueService matchmakingQueue)
    {
        var queueSize = matchmakingQueue.GetQueueSize();
        return Results.Ok(queueSize);
    }
}
