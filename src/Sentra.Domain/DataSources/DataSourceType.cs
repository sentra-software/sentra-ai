namespace Sentra.Domain.DataSources;

/// <summary>
/// Represents the type of a connected data source.
/// </summary>
public enum DataSourceType
{
    /// <summary>
    /// PostgreSQL database.
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// Microsoft SQL Server database.
    /// </summary>
    SqlServer = 2,

    /// <summary>
    /// MySQL database.
    /// </summary>
    MySql = 3
}