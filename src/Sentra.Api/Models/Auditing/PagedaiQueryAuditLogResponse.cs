namespace Sentra.Api.Models.Auditing;

/// <summary>
/// Represents a paged AI query audit log response.
/// </summary>
public sealed class PagedAiQueryAuditLogResponse
{
    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of matching records.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the audit log items.
    /// </summary>
    public IReadOnlyCollection<AiQueryAuditLogResponse> Items { get; set; } = [];
}