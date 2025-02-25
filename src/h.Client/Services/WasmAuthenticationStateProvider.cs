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

    /// <summary>
    /// Gets current claim principal. Attempts to get it from http request if not in memory.
    /// </summary>
    /// <returns></returns>
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

    public async Task MarkUserAsLoggedOut()
    {
        // Trigger a auth reload
        currentClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var currentAuth = await GetAuthenticationStateAsync();

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsAuthenticated(Claim[] claims)
    {
        currentClaimsPrincipal = NewCookiePrincipalFromClaims(claims);
    }

    private static ClaimsPrincipal NewCookiePrincipalFromClaims(Claim[] claims)
        => new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
}
