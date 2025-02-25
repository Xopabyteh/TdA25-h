using System.Security.Claims;

namespace h.Server.Infrastructure.Auth;

public static class ClaimsExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ;
        return userId is null
            ? null
            : Guid.Parse(userId);
    }

    /// <summary>
    /// If user is guest, returns the guest id, otherwise return null
    /// </summary>
    public static Guid? GetGuestId(this ClaimsPrincipal claimsPrincipal)
    {
        var guestId = claimsPrincipal.FindFirst(ClaimTypes.Anonymous)?.Value;
        return guestId is null 
            ? null 
            : Guid.Parse(guestId);    
    }
}
