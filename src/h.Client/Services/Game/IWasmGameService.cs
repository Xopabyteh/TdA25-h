using h.Contracts.Components.Services;
using h.Contracts.Games;

namespace h.Client.Services.Game;

public interface IWasmGameService : IWasmOnly
{
    Task<List<GameResponse>> LoadAllGamesAsync();
    Task DeleteGameAsync(Guid gameId);
}
