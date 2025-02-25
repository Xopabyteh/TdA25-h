using Carter;
using h.Contracts.Leaderboard;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
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
            [FromQuery] int skip,
            [FromQuery] int count,
            [FromServices] AppDbContext db
            )
        {
            // Todo: caching?
            //var entries = await db.UsersDbSet
            //    .OrderByDescending(u => u.Elo.Rating)
            //    .Skip(skip)
            //    .Take(count)
            //    .Select(u => new LeaderBoardEntryResponse(
            //        u.Username,
            //        u.Elo.Rating,
            //        u.WinAmount,
            //        u.LossAmount,
            //        Index: EF.Functions.RowNumber()
            //        ))
            //    .ToArrayAsync();

            var entries = await db.Database
                .SqlQueryRaw<LeaderBoardEntryView>(
                sql:"""
                    WITH RankedUsers AS (
                        SELECT 
                            Username, 
                            Elo_Rating AS Rating, 
                            WinAmount, 
                            LossAmount, 
                            ROW_NUMBER() OVER (ORDER BY Elo_Rating DESC) AS Rank
                        FROM UsersDbSet
                    )
                    SELECT Username, Rating, WinAmount, LossAmount, Rank 
                    FROM RankedUsers
                    WHERE Rank > @skip AND Rank <= @count
                    """,
                 new SqliteParameter("@skip", skip),
                 new SqliteParameter("@count", count))
                .AsNoTracking()
                .ToArrayAsync();

            var totalCount = await db.UsersDbSet.CountAsync();
            var responseEntries = entries
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

    /// <summary>
    /// </summary>
    /// <param name="Rank">1 indexed</param>
    public record LeaderBoardEntryView(
        string Username,
        int Rating,
        int WinAmount,
        int LossAmount,
        int Rank
    );
}