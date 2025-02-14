using Carter;
using h.Contracts.Games;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games.TdaApiSpecNecessary;

public static class GetGameById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/games/{id}", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db)
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
            game.Board.BoardMatrixToString()
        ));
    }
}
