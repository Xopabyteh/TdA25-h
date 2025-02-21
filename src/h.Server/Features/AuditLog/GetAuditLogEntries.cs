using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using h.Server.Infrastructure.Database;
using Carter;
using h.Contracts.AuditLog;
using h.Server.Infrastructure.Auth;

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
        [FromBody] GetAuditLogEntriesRequest request,
        [FromServices] AppDbContext db,
        CancellationToken cancellationToken)
    {
        var auditLogEntries = await db.AuditLogEntries
            .OrderByDescending(a => a.Id)
            .Skip(request.Pagination.Skip)
            .Take(request.Pagination.Count)
            .Select(a => new AuditLogEntryResponse(
                a.Id,
                a.CreatedAt,
                a.Message,
                a.Format,
                a.Arguments,
                a.IPAdressV4.ToString()
            ))
            .ToListAsync(cancellationToken);
        
        return Results.Ok(auditLogEntries);
    }
}