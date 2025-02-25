using Carter;
using h.Contracts.Auth;

namespace h.Server.Features.Users;

public static class GetCurrentUserClaims
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/users/current/claims", Handle)
                .RequireAuthorization();
        }
    }

    public static async Task<IResult> Handle(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var response = context.User.Claims.Select(c => new ClaimResponse(c.Type, c.Value));
        return Results.Ok(response);
    }
}
