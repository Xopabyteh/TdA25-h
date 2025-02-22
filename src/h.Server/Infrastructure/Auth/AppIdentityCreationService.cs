using h.Contracts.Auth;
using h.Primitives.Users;
using h.Server.Entities.Users;
using System.Globalization;
using System.Security.Claims;

namespace h.Server.Infrastructure.Auth;

public class AppIdentityCreationService
{
    public IReadOnlyCollection<Claim> GetClaimsForUser(User user)
    {
        var claimsFromRoles = user.Roles
        .Select(role => new Claim(ClaimTypes.Role, Enum.GetName(role)!))
        .ToArray();

        var appCustomClaimTypes = new List<Claim>(1);
        if(user.BannedFromRankedMatchmakingAt is not null)
        {
            appCustomClaimTypes.Add(new Claim(
                AppCustomClaimTypes.BannedFromRankedMatchmakingAtUTC,
                user.BannedFromRankedMatchmakingAt!.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return  [
            // Claims from user
            new Claim(ClaimTypes.NameIdentifier, user.Uuid.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),

            // App custom claims
            ..appCustomClaimTypes,

            // Roles
            ..claimsFromRoles
        ];
    } 

    public IReadOnlyCollection<Claim> GetClaimsForGuest(Guid guestId)
    {
        var name = "Návštěvník";

        return  [
            // Claims from user
            //new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Anonymous, guestId.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, nameof(UserRole.Guest))
        ];
    }

    public ClaimsPrincipal GeneratePrincipalFromClaims(IReadOnlyCollection<Claim> claims, string authenticationScheme)
    {
        var identity = new ClaimsIdentity(claims, authenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
