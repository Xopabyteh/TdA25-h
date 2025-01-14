namespace h.Client.Services;

/// <inheritdoc/>
public class WasmHttpClient : IWasmHttpClient
{
    private readonly HttpClient _httpClient;

    public WasmHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HttpClient Http => _httpClient;
}