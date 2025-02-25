using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Users;

public static class GuestLogin
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users/guest-login", Handle);
        }
    }
    public static async Task<IResult> Handle(
        [FromServices] GuestLoginService guestService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await guestService.GuestLoginAsync(httpContext);

        // Map and return
        return Results.Ok(new GuestLoginResponse(
            result.Token,
            result.GuestId,
            result.Name
        ));
    }
}
