using Sentra.Connectors.Abstractions.Connectors;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.Connectors;

/// <summary>
/// Defines a registry for resolving data connectors by connector type.
/// </summary>
public interface IConnectorRegistry
{
    /// <summary>
    /// Gets all registered connector types.
    /// </summary>
    IReadOnlyCollection<ConnectorType> RegisteredTypes { get; }

    /// <summary>
    /// Attempts to retrieve a connector for the specified type.
    /// </summary>
    /// <param name="connectorType">The connector type to resolve.</param>
    /// <returns>
    /// A successful result containing the resolved <see cref="IDataConnector"/>,
    /// or a failed result when no connector is registered for the specified type.
    /// </returns>
    Result<IDataConnector> GetConnector(ConnectorType connectorType);
}