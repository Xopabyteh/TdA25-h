using h.Server.Features.LeaderBoard;
using h.Server.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure.Leaderboard;

public class LeaderboardService
{
    private readonly AppDbContext _db;

    public LeaderboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(LeaderBoardEntryView[] entries, int totalCount)> GetEntriesAsync(int skip, int count)
    {
         var entries = await _db.Database
                .SqlQueryRaw<LeaderBoardEntryView>(
                    sql: """
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

         var totalCount = await _db.UsersDbSet.CountAsync();

        return (entries, totalCount);
    }
}
