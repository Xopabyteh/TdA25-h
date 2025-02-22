//using h.Contracts.Auth;
//using h.Primitives.Users;
//using Microsoft.AspNetCore.Authorization;

//namespace h.Client.Services;

///// <summary>
///// Policy names shall be used with <see langword="nameof(MPolicy)"/>
///// </summary>
//public static class AppPolicies
//{
//    ///// <summary>
//    ///// A user who is able to join matchmaking
//    ///// </summary>
//    public static AuthorizationPolicy AbleToJoinMatchmaking => new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
        
//        // Not admin
//        .RequireAssertion(context => !context.User.IsInRole(nameof(UserRole.Admin)))
        
//        // Not banned from ranked matchmaking
//        .RequireAssertion(context => !context.User.HasClaim(c => c.Type == AppCustomClaimTypes.BannedFromRankedMatchmakingAtUTC))
//        .Build();

//    public static AuthorizationPolicy IsAdmin => new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .RequireRole(nameof(UserRole.Admin))
//        .Build();

//    public static void AddAppPolicies(this AuthorizationOptions o)
//    {
//        // Default authentication policya
//        o.AddPolicy(nameof(AbleToJoinMatchmaking), AbleToJoinMatchmaking);
//        o.AddPolicy(nameof(IsAdmin), IsAdmin);
//    }
//}
