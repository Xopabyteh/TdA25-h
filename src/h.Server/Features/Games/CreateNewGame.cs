using Carter;
using h.Server.Entities.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class CreateNewGame
{
    public class Endpoint : ICarterModule
    {
        public readonly record struct Request(string Name, GameDifficulty Difficulty, string[][] Board);
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/games", async (
                [FromBody] Request request,
                [FromServices] AppDbContext db) =>
            {
                var board = GameBoard.Parse(request.Board);
                var game = new Game(request.Name, request.Difficulty, board);

                await db.GamesDbSet.AddAsync(game);
                await db.SaveChangesAsync();
            });
        }
    }
}
