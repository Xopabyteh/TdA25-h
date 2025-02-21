using Carter;
using h.Contracts.Leaderboard;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.LeaderBoard;

public static class GetLeaderboard
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/leaderboard", Handle);
        }

        public static async Task<IResult> Handle(
            [FromBody] GetLeaderboardRequest request,
            [FromServices] AppDbContext db
            )
        {
            // Todo: caching?
            var entries = await db.UsersDbSet
                .OrderByDescending(u => u.Elo.Rating)
                .Skip(request.Pagination.Skip)
                .Take(request.Pagination.Count)
                .Select(u => new LeaderBoardEntryResponse(u.Username, u.Elo.Rating))
                .ToArrayAsync();

            return Results.Ok(entries);
        }
    }
}