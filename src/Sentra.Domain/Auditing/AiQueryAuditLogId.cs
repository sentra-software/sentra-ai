namespace Sentra.Domain.Auditing;

/// <summary>
/// Represents the identifier of an AI query audit log entry.
/// </summary>
public readonly record struct AiQueryAuditLogId(Guid Value)
{
    /// <summary>
    /// Creates a new audit log identifier.
    /// </summary>
    /// <returns>A new <see cref="AiQueryAuditLogId"/>.</returns>
    public static AiQueryAuditLogId New() => new(Guid.NewGuid());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}