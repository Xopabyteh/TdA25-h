using Carter;
using h.Server.Entities.Games;
using Microsoft.AspNetCore.Mvc;

namespace h.Server.Features.Games;

public static class CreateNewGame
{
    public class Endpoint : ICarterModule
    {
        public readonly record struct Request(string Name, GameDifficulty Difficulty, string[][] BoardMatrix);
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/games", async (
                [FromBody] Request request) =>
            {
            });
        }
    }
}
