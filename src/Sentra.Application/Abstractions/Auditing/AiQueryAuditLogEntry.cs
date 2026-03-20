using Sentra.Domain.Auditing;

namespace Sentra.Application.Abstractions.Auditing;

/// <summary>
/// Represents a write-ready AI query audit log entry.
/// </summary>
public sealed record AiQueryAuditLogEntry(
    Guid TenantId,
    Guid IdentityUserId,
    Guid DataSourceId,
    string Question,
    string? GeneratedSql,
    int? RowCount,
    AiQueryAuditStatus Status,
    string? ErrorCode,
    string? ErrorMessage,
    int DurationMs
);