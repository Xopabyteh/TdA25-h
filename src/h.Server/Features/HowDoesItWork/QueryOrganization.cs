using Carter;
using h.Contracts.HowDoesItWork;

namespace h.Server.Features.HowDoesItWork;
/// <summary>
/// Phase 1 - "How does it work?"
/// Just the single query to return the simple object...
/// </summary>
public static class QueryOrganization
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api", () =>
            {
                return Results.Ok(new OrganizationResponse("Student Cyber Games"));
            });
        }
    }
}
