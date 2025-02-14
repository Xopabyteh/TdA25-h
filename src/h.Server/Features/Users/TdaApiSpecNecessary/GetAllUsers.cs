using Carter;
using h.Contracts.Users;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class GetAllUsers
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/users", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        CancellationToken cancellationToken)
    {
        var users = await db.UsersDbSet
            .Select(u => new UserResponse(
                u.Uuid,
                u.CreatedAt,
                u.UpdatedAt,
                u.Username,
                u.Email,
                u.Elo.Rating,
                u.WinAmount,
                u.DrawAmount,
                u.LossAmount
            ))
            .ToListAsync(cancellationToken);
        return Results.Ok(users);
    }
}
