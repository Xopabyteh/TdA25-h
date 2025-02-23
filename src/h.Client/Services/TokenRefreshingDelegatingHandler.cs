using System.Net.Http.Json;

namespace h.Client.Services;

/// <summary>
/// Attempts to refresh the token before making a request
/// if it is about to expire
/// </summary>
public class TokenRefreshingDelegatingHandler : DelegatingHandler
{
    private readonly WasmAuthenticationStateProvider _authStateProvider;

    public TokenRefreshingDelegatingHandler(WasmAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authStateProvider.GetTokenAsync();

        // No token
        if (token is null)
            return await base.SendAsync(request, cancellationToken);
        
        // No need to refresh
        if (token.ValidTo >= DateTime.UtcNow.AddMinutes(5))
            return await base.SendAsync(request, cancellationToken);

        // If this request is a token refresh, skip refreshing to avoid infinite loop
        if (request.RequestUri?.AbsolutePath.Contains("refresh-token") == true)
            return await base.SendAsync(request, cancellationToken);

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/refresh-token");
        var response = await base.SendAsync(refreshRequest, cancellationToken);

        // Couldn't refresh
        if (!response.IsSuccessStatusCode)
            return await base.SendAsync(request, cancellationToken);

        var newToken = await response.Content.ReadFromJsonAsync<string>();
        if(string.IsNullOrEmpty(newToken))
            return await base.SendAsync(request, cancellationToken);

        await _authStateProvider.MarkUserAsAuthenticated(newToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
