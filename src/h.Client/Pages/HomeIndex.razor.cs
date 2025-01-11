using h.Client.Services;
using h.Contracts.Components.Services;
using h.Contracts.Games;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace h.Client.Pages;

public partial class HomeIndex
{
    [Inject] protected IWasmOnlyHttpClient _HttpClient { get; set; } = null!;
    
    private List<GameResponse>? games = new();
    private bool gamesLoading = true;

    protected override async Task OnInitializedAsync()
    {
        // If prerendering, do not fetch data
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        games = await _HttpClient.Http!.GetFromJsonAsync<List<GameResponse>>("api/v1/games", AppJsonOptions.WithConverters);
        gamesLoading = false;
    }
}
