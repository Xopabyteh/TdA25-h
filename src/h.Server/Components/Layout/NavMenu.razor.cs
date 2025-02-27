using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace h.Server.Components.Layout;
public partial class NavMenu
{
    private bool isNavOpen = false;

    private void HandleHamburger()
    {
        isNavOpen = !isNavOpen;
    }
}