using Carter;
using h.Contracts.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Games;

public static class GetAllGames
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/games", async ([FromServices] AppDbContext db) =>
            {
                // Get from db
                var games = await db.GamesDbSet.Select(g => new {
                    g.Id,
                    g.CreatedAt,
                    g.UpdatedAt,
                    g.Name,
                    g.Difficulty,
                    g.GameState,
                    g.Board
                }).ToListAsync();

                // Map to response
                var responses = games.Select(g => new GameResponse(
                    g.Id,
                    g.CreatedAt,
                    g.UpdatedAt,
                    g.Name,
                    g.Difficulty,
                    g.GameState,
                    GameBoard.BoardMatrixToString(g.Board.BoardMatrix)
                ));

                return Results.Ok(responses);
            });
        }
    }
}
