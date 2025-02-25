using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace h.Server.Infrastructure.Auth;

public class GuestLoginService
{
    private readonly JwtTokenCreationService _tokenService;
    private readonly AppIdentityCreationService _identityService;
    public GuestLoginService(
        JwtTokenCreationService tokenService,
        AppIdentityCreationService identityService)
    {
        _tokenService = tokenService;
        _identityService = identityService;
    }
    public async Task<GuestLoginResult> GuestLoginAsync(HttpContext httpContext)
    {
        // Generate token
        var guestId = Guid.NewGuid();
        
        // Create identity
        var name = "Návštěvník";
        var claims = _identityService.GetClaimsForGuest(guestId, name);

        // Generate token
        var token = _tokenService.GenerateToken(claims);

        // Sign in with Cookie
        var principal = _identityService.GeneratePrincipalFromClaims(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });

        // Add token to response
        httpContext.Response.Headers.Append("Authorization", $"Bearer {token}");

        // Return result
        return new GuestLoginResult(token, guestId, name);
    }

    public readonly record struct GuestLoginResult(string Token, Guid GuestId, string Name);
}