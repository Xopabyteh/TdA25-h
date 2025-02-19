using Carter;
using FluentValidation;
using h.Contracts.Users;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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
        [FromServices] IConfiguration config,
        [FromServices] AppDbContext db,
        [FromServices] PasswordHashService passwordHashService,
        [FromServices] IValidator<LoginUserRequest> validator,
        [FromServices] JwtTokenService tokenService,
        [FromServices] IAuthenticationService authenticationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Generate token
        var guestId = Guid.NewGuid();
        var token = tokenService.GenerateGuestToken(guestId);

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Map and return
        return Results.Ok(new GuestLoginResponse(
            token,
            guestId
        ));
    }
}
