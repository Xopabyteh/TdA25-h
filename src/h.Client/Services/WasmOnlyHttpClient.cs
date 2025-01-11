using h.Contracts.Components.Services;

namespace h.Client.Services;

/// <inheritdoc/>
public class WasmOnlyHttpClient : IWasmOnlyHttpClient
{
    private readonly HttpClient _httpClient;

    public WasmOnlyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient Http => _httpClient;
}
