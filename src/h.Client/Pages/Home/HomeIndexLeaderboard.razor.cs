using h.Client.Services;
using h.Contracts.Leaderboard;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Home;

public partial class HomeIndexLeaderboard
{
    private readonly IHApiClient _api;


    private const int GET_TOP_COUNT = 10;
    private LeaderBoardEntryResponse[]? topNLeaderboardEntries;
    public HomeIndexLeaderboard(IHApiClient api)
    {
        _api = api;
    }

    protected override async Task OnInitializedAsync()
    {
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        var response = await _api.GetLeaderboard(0, GET_TOP_COUNT);
        topNLeaderboardEntries = response.PaginatedEntries;
    }
}
