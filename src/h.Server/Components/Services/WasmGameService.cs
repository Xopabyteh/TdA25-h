using h.Client.Services.Game;
using h.Contracts.Games;

namespace h.Server.Components.Services;

public class WasmGameService : IWasmGameService
{
    Task<GameResponse?> IWasmGameService.CreateGameAsync(CreateNewGameRequest request)
    {
        throw new NotImplementedException();
    }

    Task IWasmGameService.DeleteGameAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }

    Task<List<GameResponse>> IWasmGameService.LoadAllGamesAsync()
    {
        throw new NotImplementedException();
    }

    Task<GameResponse?> IWasmGameService.LoadGameAsync(Guid gameId)
    {
        throw new NotImplementedException();
    }

    Task<GameResponse?> IWasmGameService.UpdateGameAsync(UpdateGameRequest request)
    {
        throw new NotImplementedException();
    }
}
