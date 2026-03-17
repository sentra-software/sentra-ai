using Sentra.Domain.DataSources;

namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a request to read the schema of a data source.
/// </summary>
/// <param name="DataSourceType">The type of the data source.</param>
/// <param name="ConnectionString">The raw connection string to use.</param>
public sealed record ReadDataSourceSchemaRequest(
    DataSourceType DataSourceType,
    string ConnectionString);