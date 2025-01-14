using h.Contracts.Components.Services;
using h.Contracts.Games;

namespace h.Client.Services.Game;

public interface IWasmGameService : IWasmOnly
{
    Task<List<GameResponse>> LoadAllGamesAsync();
    Task<GameResponse?> LoadGameAsync(Guid gameId);
    Task DeleteGameAsync(Guid gameId);
    Task<GameResponse?> UpdateGameAsync(UpdateGameRequest request);
    Task<GameResponse?> CreateGameAsync(CreateNewGameRequest request);
}
