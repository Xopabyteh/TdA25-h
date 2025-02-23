using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using h.Server.Infrastructure.Database;
using Carter;
using h.Contracts.AuditLog;
using h.Server.Infrastructure.Auth;
using Microsoft.VisualBasic;

namespace h.Server.Features.AuditLog;

public static class GetAuditLogEntries
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/auditlog", Handle)
                .RequireAuthorization(nameof(AppPolicies.IsAdmin));
        }
    }

    public static async Task<IResult> Handle(
        [FromQuery] int skip,
        [FromQuery] int count,
        [FromServices] AppDbContext db,
        CancellationToken cancellationToken)
    {
        var auditLogEntries = await db.AuditLogEntries
            .OrderByDescending(a => a.Id)
            .Skip(skip)
            .Take(count)
            .Select(a => new AuditLogEntryResponse(
                a.Id,
                a.CreatedAt,
                a.Message,
                a.Format,
                a.Arguments,
                a.IPAdressV4.ToString()
            ))
            .ToArrayAsync(cancellationToken);
        
        var totalCount = await db.AuditLogEntries.CountAsync(cancellationToken);

        return Results.Ok(new AuditLogResponse(totalCount, auditLogEntries));
    }
}