using Carter;
using h.Contracts.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class GetGameById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/games/{id}", async (
                [FromRoute] Guid id,
                [FromServices] AppDbContext db) =>
            {
                var game = await db.GamesDbSet.FindAsync(id);

                if (game is null)
                {
                    return ErrorResults.NotFound();
                }

                return Results.Ok(new GameResponse(
                    game.Id,
                    game.CreatedAt,
                    game.UpdatedAt,
                    game.Name,
                    game.Difficulty,
                    game.GameState,
                    GameBoard.BoardMatrixToString(game.Board.BoardMatrix)
                ));
            });
        }
    }
}
