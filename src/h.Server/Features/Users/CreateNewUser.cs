using Carter;
using h.Primitives.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Users;

public static class CreateNewUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] IAuthenticationService authenticationService,
        HttpContext httpContext)
    {
        return Results.Ok(httpContext.User.IsInRole(nameof(UserRole.Admin)));
    }
}
