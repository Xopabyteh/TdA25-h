using h.Contracts.Components.Services;

namespace h.Client.Services.Game;

public class GameInvitationHubClientService : IWasmOnly, IAsyncDisposable
{

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
