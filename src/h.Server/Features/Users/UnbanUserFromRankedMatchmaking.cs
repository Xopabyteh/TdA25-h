using Carter;
using h.Contracts;
using h.Contracts.Users;
using h.Server.Infrastructure;
using h.Server.Infrastructure.AuditLog;
using h.Server.Infrastructure.Auth;
using h.Server.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Users;

public static class UnbanUserFromRankedMatchmaking
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/users/{uuid:guid}/unban", Handle)
                .AuditMethod(ctx => $"Unbanned user {ctx.HttpContext.GetRouteValue("uuid")}")
                .RequireAuthorization(nameof(AppPolicies.IsAdmin));
        }
    }

    public static async Task<IResult> Handle(
        [FromRoute] Guid uuid,
        [FromServices] AppDbContext db,
        [FromServices] TimeProvider timeProvider)
    {
        var user = await db.UsersDbSet.FirstOrDefaultAsync(u => u.Uuid == uuid);
        if (user is null)
            return ErrorResults.NotFound([SharedErrors.User.UserNotFound()]);

        user.BannedFromRankedMatchmakingAt = null;

        db.Update(user);
        await db.SaveChangesAsync();

        return Results.Ok();
    }
}
