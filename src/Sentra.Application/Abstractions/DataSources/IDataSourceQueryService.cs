using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.DataSources;

/// <summary>
/// Defines application services for executing queries against tenant data sources.
/// </summary>
public interface IDataSourceQueryService
{
    /// <summary>
    /// Executes a query using the connector that matches the specified data source type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the <see cref="QueryExecutionResult"/> when the connector is resolved
    /// and the query is executed; otherwise, a failed result describing the error.
    /// </returns>
    Task<Result<QueryExecutionResult>> ExecuteQueryAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string query,
        CancellationToken cancellationToken = default
    );
}