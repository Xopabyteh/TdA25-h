using h.Contracts.Components.Services;
using static h.Contracts.Components.Services.IWasmOnlyHttpClient;

namespace h.Server.Components.Services;

/// <inheritdoc/>
public class WasmOnlyHttpClient : IWasmOnlyHttpClient
{
    public HttpClient Http => throw new CannotUseWasmOnlyHttpClientOutsideWasmException();
}
