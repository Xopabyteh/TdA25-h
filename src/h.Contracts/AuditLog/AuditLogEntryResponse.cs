using System.Net;

namespace h.Contracts.AuditLog;
public readonly record struct AuditLogEntryResponse(
    int Id,
    DateTime CreatedAt,
    string Message,
    string Format,
    object?[] Arguments,
    string IPV4
);
