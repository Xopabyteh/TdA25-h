using Carter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace h.Server.Features.Users;

public static class LogoutUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users/logout", Handle)
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handle(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Clear cookie
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remove jwt header
        httpContext.Response.Headers.Remove("Authorization");

        return Results.Ok();
    }
}
