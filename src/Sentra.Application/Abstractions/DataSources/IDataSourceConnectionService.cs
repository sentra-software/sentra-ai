using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.DataSources;

/// <summary>
/// Defines application services for testing tenant data source connections.
/// </summary>
public interface IDataSourceConnectionService
{
    /// <summary>
    /// Tests a data source connection using the connector that matches the specified data source type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string to test.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the <see cref="ConnectionTestResult"/> when the connector is resolved
    /// and the connection test was executed; otherwise, a failed result describing the error.
    /// </returns>
    Task<Result<ConnectionTestResult>> TestConnectionAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default
    );
}