using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace h.Client.Services;

public class WasmAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;

    private const string TOKEN_KEY = "h.jwt-token";

    public WasmAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetItemAsync<string>(TOKEN_KEY);

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        
        var jwt = ParseToken(token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticated(string jwtToken)
    {
        await _sessionStorage.SetItemAsync(TOKEN_KEY, jwtToken);
        
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.RemoveItemAsync(TOKEN_KEY);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<JwtSecurityToken?> GetTokenAsync()
    {
        if(!await _sessionStorage.ContainKeyAsync(TOKEN_KEY))
            return null;

        var token = await _sessionStorage.GetItemAsync<string>(TOKEN_KEY);
        return ParseToken(token);
    }

    private static JwtSecurityToken ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt;
    }
}
