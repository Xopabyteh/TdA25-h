﻿using h.Contracts.Auth;
using h.Primitives.Users;
using Microsoft.AspNetCore.Authorization;

namespace h.Server.Infrastructure.Auth;

/// <summary>
/// Policy names shall be used with <see langword="nameof(MPolicy)"/>
/// </summary>
public static class AppPolicies
{
    ///// <summary>
    ///// A user who is able to join matchmaking
    ///// </summary>
    public static AuthorizationPolicy AbleToJoinMatchmaking => new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        
        // Not admin
        .RequireAssertion(context => !context.User.IsInRole(nameof(UserRole.Admin)))
        
        // Not banned from ranked matchmaking
        .RequireAssertion(context => !context.User.HasClaim(c => c.Type == AppCustomClaimTypes.BannedFromRankedMatchmakingAtUTC))
        .Build();

    /// <summary>
    /// Not guest
    /// </summary>
    public static AuthorizationPolicy IsUser => new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireAssertion(context => !context.User.IsInRole(nameof(UserRole.Guest)))
        .Build();

    public static AuthorizationPolicy IsAdmin => new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole(nameof(UserRole.Admin))
        .Build();

    public static AuthorizationPolicy IsNotAdmin => new AuthorizationPolicyBuilder()
        .RequireAssertion(context => context.User.Identity is null || !context.User.IsInRole(nameof(UserRole.Admin)))
        .Build();

    public static void AddAppPolicies(this AuthorizationOptions o)
    {
        // Default authentication policya
        o.AddPolicy("HybridAuth", new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes("HybridAuth")
            .RequireAuthenticatedUser()
            .Build()
        );

        o.AddPolicy(nameof(AbleToJoinMatchmaking), AbleToJoinMatchmaking);
        o.AddPolicy(nameof(IsAdmin), IsAdmin);
        o.AddPolicy(nameof(IsUser), IsUser);
        o.AddPolicy(nameof(IsNotAdmin), IsNotAdmin);
    }
}
