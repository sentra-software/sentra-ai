using Sentra.Domain.DataSources;

namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a request to execute a query against a data source.
/// </summary>
/// <param name="DataSourceType">The type of the data source.</param>
/// <param name="ConnectionString">The raw connection string to use.</param>
/// <param name="Query">The query to execute.</param>
public sealed record ExecuteDataSourceQueryRequest(
    DataSourceType DataSourceType,
    string ConnectionString,
    string Query);