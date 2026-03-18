namespace Sentra.Api.Models.ManagedDataSources;

/// <summary>
/// Represents a request to select the active managed data source.
/// </summary>
public sealed class SetActiveManagedDataSourceRequest
{
    /// <summary>
    /// Gets or sets the selected data source identifier.
    /// </summary>
    public Guid DataSourceId { get; set; }
}