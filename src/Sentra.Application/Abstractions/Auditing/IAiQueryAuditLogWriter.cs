namespace Sentra.Application.Abstractions.Auditing;

/// <summary>
/// Defines persistence behaviour for AI query audit logs.
/// </summary>
public interface IAiQueryAuditLogWriter
{
    /// <summary>
    /// Writes an AI query audit log entry.
    /// </summary>
    /// <param name="entry">The entry to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task WriteAsync(AiQueryAuditLogEntry entry, CancellationToken cancellationToken = default);
}