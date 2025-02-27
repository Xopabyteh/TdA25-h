using h.Server.Entities.Users;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Components.Pages;

public partial class UserGameHistory
{
    [Parameter] [FromRoute(Name = "userId")] public Guid? UserId { get; set; }

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    private User? currentUser;

    private Dictionary<Guid, string>? opponentsInGames = new();

    public UserGameHistory(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task OnInitializedAsync()
    {
        if (UserId is null)
            return;

        await using var db = await _dbContextFactory.CreateDbContextAsync();

        // Load user
        currentUser = await db.UsersDbSet
            .Include(u => u.UserToFinishedRankedGames)
            .ThenInclude(m => m.FinishedRankedGame)
            .FirstOrDefaultAsync(u => u.Uuid == UserId.Value);

        if (currentUser is null)
            return;

        // Load oponent details
        var allGames = currentUser.UserToFinishedRankedGames
            .OrderByDescending(m => m.FinishedRankedGame!.PlayedAt);

        var opponentIdsInGames = allGames
            .Select(m => m.FinishedRankedGame!.GetOpponentUserId(currentUser!.Uuid))
            .Distinct()
            .ToArray();

        opponentsInGames = await db.UsersDbSet
            .Where(u => opponentIdsInGames.Contains(u.Uuid))
            .ToDictionaryAsync(
                keySelector: u => u.Uuid,
                elementSelector: u => u.Username);
    }

    private void HandleOpenGameArchive()
    {
        Console.WriteLine("ure gay");
    }
}
