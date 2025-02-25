using h.Client.Services;
using h.Contracts.Leaderboard;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Home;

public partial class Leaderboard
{
    private readonly IHApiClient _api;

    public Leaderboard(IHApiClient api)
    {
        _api = api;
    }

    private async ValueTask<ItemsProviderResult<LeaderBoardEntryResponse>> GetRows(ItemsProviderRequest request)
    {
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return new();

        var response = await _api.GetLeaderboard(request.StartIndex, request.Count);

        return new ItemsProviderResult<LeaderBoardEntryResponse>(response.PaginatedEntries, response.TotalCount);
    }
}
