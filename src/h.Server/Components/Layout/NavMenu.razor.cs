using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace h.Server.Components.Layout;
public partial class NavMenu
{
    [CascadingParameter]
    public Task<AuthenticationState>? authenticationState { get; set; }

    private ClaimsPrincipal? user;

    override protected async Task OnInitializedAsync()
    {
        if(authenticationState is null)
            return;

        var authState = await authenticationState;
        user = authState.User;
    }
}