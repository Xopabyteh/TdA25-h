using h.Client.Services.Game;
using h.Contracts;
using h.Contracts.Games;
using OneOf;

namespace h.Server.Components.Services;

public class WasmGameService : IWasmGameService
{
    public Task<OneOf<GameResponse, ErrorResponse>> CreateGameAsync(CreateNewGameRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<GameResponse, ErrorResponse>> UpdateGameAsync(UpdateGameRequest request)
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
}
