namespace Sentra.Api.Models.Auditing;

/// <summary>
/// Represents aggregated AI query audit statistics. 
/// </summary>
public sealed class AiQueryAuditStatsResponse
{
    /// <summary>
    /// Gets or sets the total number of queries.
    /// </summary>
    public int TotalQueries { get; set; }

    /// <summary>
    /// Gets or sets the number of successful queries.
    /// </summary>
    public int SucceededCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed queries.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of blocked queries.
    /// </summary>
    public int BlockedCount { get; set; }

    /// <summary>
    /// Gets or sets the success rate percentage.
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Gets or sets the average duration in milliseconds.
    /// </summary>
    public decimal AvgDurationMs { get; set; }

    /// <summary>
    /// Gets or sets the number of queries in the last 24 hours.
    /// </summary>
    public int QueriesLast24Hours { get; set; }
}