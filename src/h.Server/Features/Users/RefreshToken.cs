using Carter;
using h.Contracts;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users;

/// <summary>
/// Assumes a persistent cookie is used, so the user can be identified by the cookie.
/// Returns a new token for the user.
/// </summary>
public static class RefreshToken
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users/refresh-token", Handle)
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromServices] JwtTokenCreationService tokenService,
        [FromServices] AppIdentityCreationService identityService,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var guestId = context.User.GetGuestId();
        if (guestId is not null)
        {
            // Refresh guest token
            var guestClaims = identityService.GetClaimsForGuest(guestId.Value, context.User.Identity!.Name!);
            var guestToken = tokenService.GenerateToken(guestClaims);
            
            context.Response.Headers["Authorization"] = guestToken;

            return Results.Ok(guestToken);
        }

        // -> User
        var userId = context.User.GetUserId();
        var user = await db.UsersDbSet
            .FirstOrDefaultAsync(u => u.Uuid == userId, cancellationToken);
        
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);
        
        var claims = identityService.GetClaimsForUser(user);

        var token = tokenService.GenerateToken(claims);
        context.Response.Headers["Authorization"] = token;

        return Results.Ok(token);
    }
}
