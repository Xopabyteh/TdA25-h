using Carter;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class DeleteGame
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/games/{id}", async (
                [FromRoute] Guid id,
                [FromServices] AppDbContext db) =>
            {
                var game = await db.GamesDbSet.FindAsync(id);

                if (game is null)
                    return ErrorResults.NotFound();

                db.GamesDbSet.Remove(game);
                await db.SaveChangesAsync();

                return Results.StatusCode(204);
            });
        }
    }
}
