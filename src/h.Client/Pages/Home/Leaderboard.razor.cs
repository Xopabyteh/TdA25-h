using h.Client.Services;
using h.Contracts;
using h.Contracts.Leaderboard;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Home;

public partial class Leaderboard
{
    private readonly IHApiClient _api;


    private const int GET_TOP_COUNT = 10;
    private LeaderBoardEntryResponse[]? leaderBoardEntries;
    public Leaderboard(IHApiClient api)
    {
        _api = api;
    }

    protected override async Task OnInitializedAsync()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        leaderBoardEntries = await _api.GetLeaderboard(0, GET_TOP_COUNT);
    }
}
