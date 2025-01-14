using h.Contracts.Components.Services;
using h.Contracts.Games;
using System.Net.Http.Json;

namespace h.Client.Services.Game;

public class WasmGameService : IWasmGameService
{
    private readonly IWasmHttpClient _httpClient;

    public WasmGameService(IWasmHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task DeleteGameAsync(Guid gameId)
    {
        return _httpClient.Http!.DeleteAsync($"api/v1/games/{gameId}")!;
    }

    public Task<List<GameResponse>> LoadAllGamesAsync()
    {
        return _httpClient.Http!.GetFromJsonAsync<List<GameResponse>>("api/v1/games", AppJsonOptions.WithConverters)!;
    }
}
