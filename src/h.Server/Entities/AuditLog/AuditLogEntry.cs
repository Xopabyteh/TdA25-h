using System.Net;

namespace h.Server.Entities.AuditLog;

public class AuditLogEntry
{
    public int Id { get; }
    public DateTime CreatedAt { get; }
    /// <summary>
    /// The full message of the audit log entry. Where arguments are replaced with their values.
    /// </summary>
    public required string Message { get; init; }
    /// <summary>
    /// Same as <see cref="FormattableString.Format"/>
    /// </summary>
    public required string Format { get; set; }
    /// <summary>
    /// Same as <see cref="FormattableString.GetArguments()"/>
    /// </summary>
    public required object?[] Arguments { get; set; }
    public required IPAddress IPAdressV4 { get; init; }
}
