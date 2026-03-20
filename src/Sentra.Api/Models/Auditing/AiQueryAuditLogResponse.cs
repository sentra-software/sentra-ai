namespace Sentra.Api.Models.Auditing;

/// <summary>
/// Represents a single AI query audit log response item.
/// </summary>
public sealed class AiQueryAuditLogResponse
{
    /// <summary>
    /// Gets or sets the audit log identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identity user identifier.
    /// </summary>
    public Guid IdentityUserId { get; set; }

    /// <summary>
    /// Gets or sets the data source identifier.
    /// </summary>
    public Guid DataSourceId { get; set; }

    /// <summary>
    /// Gets or sets the original natural-language question.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated SQL.
    /// </summary>
    public string? GeneratedSql { get; set; }

    /// <summary>
    /// Gets or sets the returned row count.
    /// </summary>
    public int? RowCount { get; set; }

    /// <summary>
    /// Gets or sets the audit status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}