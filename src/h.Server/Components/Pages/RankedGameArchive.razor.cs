using h.Server.Entities.MultiplayerGames;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Components.Pages;

public partial class RankedGameArchive
{
    [Parameter] [FromRoute(Name = "gameId")] public int? GameId { get; set; }

    private FinishedRankedGame? game;
    private string player1Name;
    private string player2Name;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public RankedGameArchive(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task OnInitializedAsync()
    {
        if (GameId is null)
        {
            return;
        }

        await using var db = _dbContextFactory.CreateDbContext();
        game = await db.Set<FinishedRankedGame>()
            .Include(g => g.UserToFinishedRankedGames)
            .ThenInclude(u => u.User)
            .FirstOrDefaultAsync(g => g.Id == GameId);

        if (game is null)
            return;

        //player1Name = game.UserToFinishedRankedGames.First(u => u.UserId == game.Player1Id)!.User!.Username;
        player1Name = game.UserToFinishedRankedGames.FirstOrDefault(u => u.UserId == game.Player1Id)?.User?.Username
            ?? "Neznámý uživatel";
        player2Name = game.UserToFinishedRankedGames.FirstOrDefault(u => u.UserId == game.Player2Id)?.User?.Username
            ?? "Neznámý uživatel";
    }
}
