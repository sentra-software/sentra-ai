using Sentra.Application.Abstractions.Connectors;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Connectors;

/// <summary>
/// Represents the default in-memory connector registry.
/// </summary>
public sealed class ConnectorRegistry : IConnectorRegistry
{
    private readonly IReadOnlyDictionary<ConnectorType, IDataConnector> _connectors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectorRegistry"/> class.
    /// </summary>
    /// <param name="connectors">The available connector implementations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectors"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate connector types are registered.</exception>
    public ConnectorRegistry(IEnumerable<IDataConnector> connectors)
    {
        ArgumentNullException.ThrowIfNull(connectors);

        List<IDataConnector>? connectorList = connectors.ToList();
        IGrouping<ConnectorType, IDataConnector>? duplicateType = connectorList
            .GroupBy(x => x.Type)
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateType is not null)
        {
            throw new InvalidOperationException(
                $"Multiple connectors are registered for type '{duplicateType.Key}'.");
        }

        _connectors = connectorList.ToDictionary(x => x.Type, x => x);
    }

    /// <summary>
    /// Gets all registered connector types.
    /// </summary>
    public IReadOnlyCollection<ConnectorType> RegisteredTypes => _connectors.Keys.ToArray();

    /// <summary>
    /// Attempts to retrieve a connector for the specified type.
    /// </summary>
    /// <param name="connectorType">The connector type to resolve.</param>
    /// <returns>
    /// A successful result containing the resolved <see cref="IDataConnector"/>,
    /// or a failed result when no connector is registered.
    /// </returns>
    public Result<IDataConnector> GetConnector(ConnectorType connectorType)
    {
        if (_connectors.TryGetValue(connectorType, out IDataConnector? connector))
        {
            return Result.Success(connector);
        }

        return Result.Failure<IDataConnector>(
            Error.Failure(
                "connectors.not_registered",
                $"No connector is registered for type '{connectorType}'."));
    }
}