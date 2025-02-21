namespace h.Server.Infrastructure.Auth;

public static class AppCustomClaimTypes
{
    /// <summary>
    /// Value is a UTC datetime in invariant culture
    /// </summary>
    public const string BannedFromRankedMatchmakingAtUTC = "is_banned";
}