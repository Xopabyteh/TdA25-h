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

    public async Task<GameResponse?> CreateGameAsync(CreateNewGameRequest request)
    {
        var result = await _httpClient.Http!.PostAsJsonAsync("api/v1/games", request, AppJsonOptions.WithConverters);
        
        if(result.StatusCode != System.Net.HttpStatusCode.Created)
            return null;

        return await result.Content.ReadFromJsonAsync<GameResponse>(AppJsonOptions.WithConverters);
    }

    public Task DeleteGameAsync(Guid gameId)
    {
        return _httpClient.Http!.DeleteAsync($"api/v1/games/{gameId}")!;
    }

    public Task<List<GameResponse>> LoadAllGamesAsync()
    {
        return _httpClient.Http!.GetFromJsonAsync<List<GameResponse>>("api/v1/games", AppJsonOptions.WithConverters)!;
    }

    public async Task<GameResponse?> LoadGameAsync(Guid gameId)
    {
        try
        {
            var response = await _httpClient.Http!.GetFromJsonAsync<GameResponse>($"api/v1/games/{gameId}", AppJsonOptions.WithConverters);
            return response;
        }
        catch (HttpRequestException e)
        {
            if(e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            throw;
        }
    }

    public async Task<GameResponse?> UpdateGameAsync(UpdateGameRequest request)
    {
        var result = await _httpClient.Http!.PutAsJsonAsync($"api/v1/games/{request.GameId}", request, AppJsonOptions.WithConverters)!;
        if(result.StatusCode != System.Net.HttpStatusCode.OK)
            return null;

        return await result.Content.ReadFromJsonAsync<GameResponse?>(AppJsonOptions.WithConverters);
    }
}
