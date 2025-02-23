﻿using h.Contracts.Users;
using h.Primitives.Users;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace h.Client.Services;

/// <summary>
/// Provides state of the current user for the wasm client.
/// Is this the best way? LET'S FIND OUT!
/// </summary>
public class WasmCurrentUserStateService : IWasmCurrentUserStateService 
{
    public string Name { get; private set; } = null!;
    public bool IsGuest { get; set; }
    public UserResponse? UserDetails { get; set; }
    private bool shouldRefresh = true;

    private readonly IHApiClient _api;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    
    public WasmCurrentUserStateService(IHApiClient api, AuthenticationStateProvider authenticationStateProvider)
    {
        _api = api;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task EnsureStateAsync()
    {
        if (!shouldRefresh)
            return;

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user is not {Identity: not null})
            throw new UserNotAuthenticatedException();

        shouldRefresh = false;

        IsGuest = user.IsInRole(nameof(UserRole.Guest));
        if(IsGuest)
        {
            // -> Guest
            UserDetails = null;
            Name = user.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            return;
        }

        // -> User
        var userDetails = await _api.GetCurrentUser();
        UserDetails = userDetails;
        Name = userDetails.Username;
    }

    public void MarkShouldRefresh()
    {
        shouldRefresh = true;
    }

    public class UserNotAuthenticatedException : Exception
    {
        public UserNotAuthenticatedException() 
            : base("User is not authenticated as a user, nor as a guest, so no state can be formed")
        {
        }
    }
}