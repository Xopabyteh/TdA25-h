using h.Server.Infrastructure.Auth;
using System.Security.Claims;

namespace h.Server.Infrastructure.MultiplayerGames;

/// <summary>
/// Since games allow playing of unregistered users, but they need to maintain
/// some sort of identity, we create an identity for this usecase.
/// </summary>
/// <param name="SessionId">Identity specific to the multiplayer session</param>
/// <param name="IsGuest">Is user authenticated or guest?</param>
/// <param name="UserId">If user is authenticated, this is his userId</param>
public readonly record struct MultiplayerGameUserIdentity(
    Guid SessionId,
    bool IsGuest,
    Guid? UserId,
    string Name
)
{
    public static MultiplayerGameUserIdentity FromGuest(Guid guestId, string name)
        => new(
            SessionId: guestId,
            IsGuest: true,
            UserId: null,
            name
        );

    public static MultiplayerGameUserIdentity FromUserId(Guid userId, string name)
        => new(
              SessionId: userId,
              IsGuest: false,
              userId,
              name
        );

    public static MultiplayerGameUserIdentity FromNETIdentity(ClaimsPrincipal user)
    {
        // Is guest?
        var guestId = user.GetGuestId();
        if(guestId is not null)
            return FromGuest(guestId.Value, user.Identity!.Name!);

        // User
        var userId = user.GetUserId();
        if(userId is null)
            throw new InvalidOperationException("User is not authenticated nor as guest nor as user");

        return FromUserId(userId.Value, user.Identity!.Name!);
    }
}