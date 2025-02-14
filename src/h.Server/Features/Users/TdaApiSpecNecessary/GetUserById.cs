using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class GetUserById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/users/{id}", Handle);
        }
    }
    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var user = await db.UsersDbSet
            .FirstOrDefaultAsync(u => u.Uuid == id, cancellationToken);
        
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);
        
        var response = new UserResponse(
            user.Uuid,
            user.CreatedAt,
            user.UpdatedAt,
            user.Username,
            user.Email,
            user.Elo.Rating,
            user.WinAmount,
            user.DrawAmount,
            user.LossAmount
        );

        return Results.Ok(response);
    }
}
