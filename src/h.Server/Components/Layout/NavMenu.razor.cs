namespace h.Server.Components.Layout;
public partial class NavMenu
{
    private bool isNavOpen = false;

    private void HandleHamburger()
    {
        isNavOpen = !isNavOpen;
    }
}