
using h.Server.Infrastructure.Auth;

namespace h.Server.Infrastructure.Middleware;

/// <summary>
/// Temporarily ensures that every user of app is atleast guest level authenticated.
/// </summary>
public class GuestLoginEnsureMiddleware : IMiddleware
{
    private readonly GuestLoginService _guestLoginService;

    public GuestLoginEnsureMiddleware(GuestLoginService guestLoginService)
    {
        _guestLoginService = guestLoginService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if(context.User is {Identity: {IsAuthenticated: true}})
        {
            await next(context);
            return;
        }

        // Guest login
        await _guestLoginService.GuestLoginAsync(context);

        await next(context);
    }
}
