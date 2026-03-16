namespace Sentra.Connectors.Abstractions.Connectors;

/// <summary>
/// Represents the supported connector types within Sentra.
/// </summary>
public enum ConnectorType
{
    /// <summary>
    /// PostgreSQL connector.
    /// </summary>
    PostgreSql = 1,

    /// <summary>
    /// Microsoft SQL Server connector.
    /// </summary>
    SqlServer = 2,

    /// <summary>
    /// MySQL connector.
    /// </summary>
    MySql = 3
}