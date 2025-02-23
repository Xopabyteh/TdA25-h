using h.Contracts.Users;
using h.Primitives.Users;
using Microsoft.AspNetCore.Components.Authorization;

namespace h.Client.Services;

public class CurrentUserStateService
{
    public string Name { get; private set; }
    public bool IsGuest { get; set; }
    public UserResponse? UserDetails { get; set; }


    private readonly IHApiClient _api;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public CurrentUserStateService(IHApiClient api, AuthenticationStateProvider authenticationStateProvider)
    {
        _api = api;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task EnsureStateAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user is not {Identity: not null})
            throw new UserNotAuthenticatedException();

        IsGuest = user.IsInRole(nameof(UserRole.Guest));

    }

    public class UserNotAuthenticatedException : Exception
    {
        public UserNotAuthenticatedException() 
            : base("User is not authenticated as a user, nor as a guest, so no state can be formed")
        {
        }
    }
}