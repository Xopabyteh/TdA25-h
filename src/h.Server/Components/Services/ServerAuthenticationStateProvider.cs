//using Microsoft.AspNetCore.Components.Authorization;
//using System.Security.Claims;

//namespace h.Server.Components.Services;

//public class ServerAuthenticationStateProvider : AuthenticationStateProvider
//{
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    public ServerAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
//    {
//        _httpContextAccessor = httpContextAccessor;
//    }
//    public override Task<AuthenticationState> GetAuthenticationStateAsync()
//    {
//        // Server side
//        if (_httpContextAccessor.HttpContext is null)
//            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

//        var user = _httpContextAccessor.HttpContext.User;
//        return Task.FromResult(new AuthenticationState(user));
//    }
//}
