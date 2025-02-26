using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace h.Server.Features.Users;

public static class FindUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/users/find", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromQuery] string query,
        [FromServices] AppDbContext db,
        CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(query))
            return ErrorResults.BadRequest("Query is required");

        var isGuid = Guid.TryParse(query, out var guid);

        var entity = isGuid
            ? await db.UsersDbSet
                .FirstOrDefaultAsync(
                    u => u.Uuid == guid,
                    cancellationToken
                )
            : await db.UsersDbSet
                .FirstOrDefaultAsync(
                    u => u.Username == query
                    || u.Email == query,
                    cancellationToken
                );

        if (entity is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);

        var response = new UserResponse(
            entity.Uuid,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Username,
            entity.Email,
            entity.Elo.Rating,
            entity.WinAmount,
            entity.DrawAmount,
            entity.LossAmount,
            entity.BannedFromRankedMatchmakingAt
        );

        return Results.Ok(response);
    }
}
