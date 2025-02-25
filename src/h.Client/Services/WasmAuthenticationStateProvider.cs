using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace h.Client.Services;

public class WasmAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHApiClient _api;

    public WasmAuthenticationStateProvider(IHApiClient api)
    {
        _api = api;
    }

    private ClaimsPrincipal currentClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if(currentClaimsPrincipal is not {Identity: {IsAuthenticated: true } }) {
            // Try get current user from http request 
            var claimDTOs = await _api.GetCurrentUserClaims();
            if (claimDTOs is {Length: > 0})
            {
                var claims = claimDTOs.Select(c => new Claim(c.Type, c.Value)).ToArray();
                currentClaimsPrincipal = NewCookiePrincipalFromClaims(claims);
            }
        }

        return new AuthenticationState(currentClaimsPrincipal);
    }

    public async Task MarkUserAsAuthenticated(string jwtToken)
    {
        //await _sessionStorage.SetItemAsync(TOKEN_KEY, jwtToken);
        
        //NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        currentClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsAuthenticated(Claim[] claims)
    {
        currentClaimsPrincipal = NewCookiePrincipalFromClaims(claims);
    }

    private static ClaimsPrincipal NewCookiePrincipalFromClaims(Claim[] claims)
        => new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
}
