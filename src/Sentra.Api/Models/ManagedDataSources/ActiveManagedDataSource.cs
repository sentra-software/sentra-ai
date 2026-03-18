namespace Sentra.Api.Models.ManagedDataSources;

/// <summary>
/// Represents the active managed data source for the current user.
/// </summary>
public sealed class ActiveManagedDataSourceResponse
{
    /// <summary>
    /// Gets or sets the active data source identifier.
    /// </summary>
    public Guid? DataSourceId { get; set; }

    /// <summary>
    /// Gets or sets the active data source name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the active data source type.
    /// </summary>
    public string? DataSourceType { get; set; }

    /// <summary>
    /// Gets or sets the active data source status.
    /// </summary>
    public string? Status { get; set; }
}