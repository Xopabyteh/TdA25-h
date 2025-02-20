using Carter;
using h.Contracts.GameInvitations;
using h.Server.Infrastructure;
using h.Server.Infrastructure.GameInvitations;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Features.GameInvitations;

public static class JoinGameByInvitationCode
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/invitation/join/{roomCode:int}", Handle)
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handle(
        [FromRoute] int roomCode,
        [FromServices] InMemoryInvitationCodeService invitationCodeService,
        [FromServices] IMultiplayerGameSessionService multiplayerGameSessionService,
        [FromServices] IHubContext<GameInvitationHub, IGameInvitationHubClient> hubContext,
        [FromServices] IHubUserIdMappingService<GameInvitationHub, MultiplayerGameUserIdentity> hubUserIdMapping,
        HttpContext httpContext)
    {
        var multiplayerIdentity = MultiplayerGameUserIdentity.FromNETIdentity(httpContext.User);

        var joinRoomResult = invitationCodeService.JoinRoom(roomCode, multiplayerIdentity);
        if(joinRoomResult.IsError)
            return ErrorResults.FromFirstError(joinRoomResult.FirstError);

        var isRoomFull = joinRoomResult.Value;

        if(!isRoomFull)
            return Results.Ok(); // This should never happen, as we only have 2 players...

        // -> Room full, start game (this should always happen)
        var room = invitationCodeService.GetRoom(roomCode);
        if(room is null)
            throw new ArgumentNullException(nameof(room)); // This should never happen

        // Todo:
        // If missing is owner, kill match and return 404
        // If missing is player, remove him from the room and return error response
        // Todo: handle potential of a player leaving mid-acceptance
        var connectionIds = room.Players.Select(u 
            => hubUserIdMapping.GetConnectionId(u)
            ?? throw IHubUserIdMappingService<GameInvitationHub, MultiplayerGameUserIdentity>.UserNotPresentException(u)
        );

        // -> Success, start game
        var gameSession = multiplayerGameSessionService.CreateGameSession(room.Players);

        // Notify players about the game session
        await hubContext.Clients.Clients(connectionIds).NewGameSessionCreated(gameSession.Id);

        // Kill room
        invitationCodeService.RemoveRoom(roomCode);

        return Results.Ok();
    }
}
