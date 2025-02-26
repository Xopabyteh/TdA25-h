using h.Server.Features.LeaderBoard;
using h.Server.Infrastructure.Leaderboard;

namespace h.Server.Components.Pages.Home;

public partial class HomeIndexLeaderboard
{
    private const int GET_TOP_COUNT = 10;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    private LeaderBoardEntryView[]? topNLeaderboardEntries;

    public HomeIndexLeaderboard(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }


    protected override async Task OnInitializedAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var leaderboardService = scope.ServiceProvider.GetRequiredService<LeaderboardService>();

        var result = await leaderboardService.GetEntriesAsync(0, GET_TOP_COUNT);
        topNLeaderboardEntries = result.entries;
    }
}
