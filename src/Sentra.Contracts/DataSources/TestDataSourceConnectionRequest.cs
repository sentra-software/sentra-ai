using Sentra.Domain.DataSources;

namespace Sentra.Contracts.DataSources;

/// <summary>
/// Represents a request to test a data source connection.
/// </summary>
/// <param name="DataSourceType">The type of the data source.</param>
/// <param name="ConnectionString">The raw connection string to test.</param>
public sealed record TestDataSourceConnectionRequest(
    DataSourceType DataSourceType,
    string ConnectionString);