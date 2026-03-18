namespace Sentra.Api.Models.ManagedDataSources;

/// <summary>
/// Represents a managed data source response.
/// </summary>
public sealed class ManagedDataSourceResponse
{
    /// <summary>
    /// Gets or sets the data source identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the data source name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data source type.
    /// </summary>
    public string DataSourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}