using Carter;
using h.Contracts.Games;
using h.Server.Entities.Games;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class CreateNewGame
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/games", async (
                [FromBody] CreateNewGameRequest request,
                [FromServices] AppDbContext db) =>
            {
                // Map to entity
                var board = GameBoard.Parse(request.Board);
                var game = new Game(request.Name, request.Difficulty, board);

                // Persist
                await db.GamesDbSet.AddAsync(game);
                await db.SaveChangesAsync();

                // Map to response
                var response = new GameResponse(
                    game.Id,
                    game.CreatedAt,
                    game.UpdatedAt,
                    game.Name,
                    game.Difficulty,
                    game.GameState,
                    GameBoard.BoardMatrixToString(game.Board.BoardMatrix));
                
                return Results.Created($"/api/games/{game.Id}", response);
            });
        }
    }
}
