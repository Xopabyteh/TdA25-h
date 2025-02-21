using h.Contracts.Matchmaking;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Carter;
using h.Server.Infrastructure.MultiplayerGames;

namespace h.Server.Features.Matchmaking;

public static class AcceptMatch
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/matchmaking/accept/{matchingId:guid}", Handle)
                .RequireAuthorization(nameof(AppPolicies.AbleToJoinMatchmaking));
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] InMemoryMatchmakingService matchmakingService,
        [FromServices] IHubUserIdMappingService<MatchmakingHub> hubUserIdMappingService,
        [FromServices] IHubContext<MatchmakingHub, IMatchmakingHubClient> hubContext,
        [FromServices] IMultiplayerGameSessionService multiplayerGameSessionService,
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

                // Create game session if everyone accepted
                if (remainingAccepteesCount == 0)
                {
                    // Create game session
                    var gameSession = multiplayerGameSessionService.CreateGameSession(
                        matching.Value.GetPlayersInMatch());

                    // Notify players about the game session
                    await hubContext.Clients.Clients(connectionIds).NewGameSessionCreated(gameSession.Id);
                }

                return Results.Ok();
            },
            error => Task.FromResult(ErrorResults.FromFirstError(error))
        );
    }   
}
