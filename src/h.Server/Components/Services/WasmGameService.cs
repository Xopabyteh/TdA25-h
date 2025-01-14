using h.Client.Services.Game;
using h.Contracts.Games;

namespace h.Server.Components.Services;

public class WasmGameService : IWasmGameService
{
    public Task DeleteGameAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }

    public Task<List<GameResponse>> LoadAllGamesAsync()
    {
        throw new NotImplementedException();
    }
}
