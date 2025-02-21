using h.Server.Entities.AuditLog;
using h.Server.Infrastructure.Database;
using System.Net;

namespace h.Server.Infrastructure.AuditLog;

/// <summary>
/// The audit log contains all actions performed by an admin.
/// </summary>
public class AuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(FormattableString action, IPAddress fromIp)
    {
        // Todo: optimalize saving?

        // Log the message
        await _db.AuditLogEntries.AddAsync(new AuditLogEntry
        {
            Message = action.ToString(),
            Format = action.Format,
            Arguments = action.GetArguments(),
            IPAdressV4 = fromIp
        });

        await _db.SaveChangesAsync();
    }
}
