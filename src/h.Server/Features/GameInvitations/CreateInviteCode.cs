using Carter;
using h.Contracts;
using h.Server.Infrastructure;
using h.Server.Infrastructure.GameInvitations;
using h.Server.Infrastructure.MultiplayerGames;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.GameInvitations;

public static class CreateInviteCode
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/invitation/create", Handle)
                .RequireAuthorization();
        }
    }

    public static IResult Handle(
        [FromServices] InMemoryInvitationCodeService invitationCodeService,
        [FromServices] IHubUserIdMappingService<GameInvitationHub, MultiplayerGameUserIdentity> hubUserIdMapping,
        HttpContext httpContext)
    {
        var multiplayerIdentity = MultiplayerGameUserIdentity.FromNETIdentity(httpContext.User);
        
        // Ensure player is in hub, otherwise tell them they must be there...
        var connectionId = hubUserIdMapping.GetConnectionId(multiplayerIdentity);
        if(connectionId is null)
        {
            return ErrorResults.Conflit($"You must be connected to {nameof(GameInvitationHub)}");
        }

        var inviteCode = invitationCodeService.CreateNewRoom(multiplayerIdentity);

        return Results.Created($"/api/v1/invitation/join/{inviteCode}", inviteCode);
    }
}
