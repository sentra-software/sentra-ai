namespace Sentra.Domain.DataSources;

/// <summary>
/// Represents the connection status of a data source.
/// </summary>
public enum DataSourceStatus
{
    /// <summary>
    /// Data source has been registered but not validated.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Data source connection is verified and operational.
    /// </summary>
    Active = 2,

    /// <summary>
    /// Data source is disabled or unreachable.
    /// </summary>
    Disabled = 3
}