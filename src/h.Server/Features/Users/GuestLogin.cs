using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        [FromServices] JwtTokenCreationService tokenService,
        [FromServices] AppIdentityCreationService identityService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        // Generate token
        var guestId = Guid.NewGuid();
        
        // Create identity
        var name = "Návštěvník";
        var claims = identityService.GetClaimsForGuest(guestId, name);

        // Generate token
        var token = tokenService.GenerateToken(claims);

        // Sign in with Cookie
        var principal = identityService.GeneratePrincipalFromClaims(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Map and return
        return Results.Ok(new GuestLoginResponse(
            token,
            guestId,
            name
        ));
    }
}
