using h.Client.Services;
using static h.Client.Services.IWasmHttpClient;

namespace h.Server.Components.Services;

/// <inheritdoc/>
public class WasmHttpClient : IWasmHttpClient
{
    public HttpClient Http => throw new CannotUseWasmOnlyHttpClientOutsideWasmException();
}
