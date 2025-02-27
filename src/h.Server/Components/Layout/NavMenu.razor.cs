using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using h.Server.Entities.Users;

namespace h.Server.Components.Layout;
public partial class NavMenu
{
    private bool isNavOpen = false;

    protected User? currentUser;

    override protected async Task OnInitializedAsync()
    {
        if(authenticationState is null)
            return;

        var authState = await authenticationState;
        user = authState.User;
    }

    private void HandleHamburger()
    {
        isNavOpen = !isNavOpen;
    }
}