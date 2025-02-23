namespace h.Contracts.AuditLog;

public readonly record struct AuditLogResponse(int TotalCount, AuditLogEntryResponse[] PaginatedEntries); 
