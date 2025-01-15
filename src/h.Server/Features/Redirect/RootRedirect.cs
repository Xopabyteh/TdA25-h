using Carter;
using h.Client.Pages;

namespace h.Server.Features.Redirect;

/// <summary>
/// Temporarily redirects root to game list,
/// as there is no root page yet...
/// </summary>
public static class RootRedirect
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/", () =>
            {
                return Results.Redirect(PageRoutes.Game.GameList);
            });
        }
    }
}
