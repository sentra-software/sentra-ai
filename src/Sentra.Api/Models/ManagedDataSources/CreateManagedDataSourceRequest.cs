namespace Sentra.Api.Models.ManagedDataSources;

/// <summary>
/// Represents a request to create a managed data source.
/// </summary>
public sealed class CreateManagedDataSourceRequest
{
    /// <summary>
    /// Gets or sets the display name of the data source.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data source type.
    /// </summary>
    public string DataSourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plaintext connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}