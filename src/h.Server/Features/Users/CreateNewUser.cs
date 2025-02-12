using Carter;

namespace h.Server.Features.Users;

public static class CreateNewUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/users", Handle);
        }
    }

    public static async Task<IResult> Handle()
    {
        return Results.Ok();
    }
}
