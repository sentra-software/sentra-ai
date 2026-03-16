namespace Sentra.Connectors.Abstractions.Connectors;

/// <summary>
/// Represents a capability that a connector can support.
/// </summary>
public enum ConnectorCapability
{
    /// <summary>
    /// Supports connectivity validation.
    /// </summary>
    TestConnection = 1,

    /// <summary>
    /// Supports schema discovery.
    /// </summary>
    ReadSchema = 2,

    /// <summary>
    /// Supports query execution.
    /// </summary>
    ExecuteQuery = 3
}