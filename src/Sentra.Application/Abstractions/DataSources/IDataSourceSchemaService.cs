using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.DataSources;

/// <summary>
/// Defines application services for reading schemas from tenant data sources.
/// </summary>
public interface IDataSourceSchemaService
{
    /// <summary>
    /// Reads the schema for the specified data source type using the matching connector.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the discovered tables when the connector is resolved
    /// and schema discovery succeeds; otherwise, a failed result describing the error.
    /// </returns>
    Task<Result<IReadOnlyCollection<TableSchema>>> ReadSchemaAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default
    );
}