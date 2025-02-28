using Microsoft.AspNetCore.Components;

namespace h.Client.Services;

public static class NavigationManagerExtensions
{
    public static string RelativeUri(this NavigationManager navigationManager)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        return uri.PathAndQuery;
    }
}
