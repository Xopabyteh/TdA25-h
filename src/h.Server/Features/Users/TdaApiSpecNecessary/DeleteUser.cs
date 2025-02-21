using Carter;
using h.Contracts;
using h.Primitives.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users.TdaApiSpecNecessary;

public static class DeleteUser
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/v1/users/{id}", Handle);
        }
    }

    public static async Task<IResult> Handle(
        [FromServices] AppDbContext db,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Uuid == id, cancellationToken);
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);

        if(user.Roles.Contains(UserRole.Admin))
            return ErrorResults.Conflit("Cannot delete admin user"); // Todo: convert to shared error if needed

        db.UsersDbSet.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Results.StatusCode(204);
    }
}
