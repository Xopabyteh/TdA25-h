using Carter;
using h.Contracts.Leaderboard;
using h.Server.Infrastructure.Leaderboard;
using Microsoft.AspNetCore.Mvc;

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
            [FromQuery] int skip,
            [FromQuery] int count,
            [FromServices] LeaderboardService leaderboardService
            )
        {
            // Todo: caching?
            var (entries, totalCount) = await leaderboardService.GetEntriesAsync(skip, count);

            var responseEntries = entries!
                .Select(entry => new LeaderBoardEntryResponse(
                entry.Username,
                entry.Rating,
                entry.WinAmount,
                entry.LossAmount,
                entry.Rank
            )).ToArray();

            return Results.Ok(new LeaderBoardResponse(responseEntries, totalCount));
        }
    }
}