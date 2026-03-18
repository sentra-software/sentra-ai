using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Represents a configurable test implementation of <see cref="IDataSourceQueryService"/>.
/// </summary>
internal sealed class TestDataSourceQueryService : IDataSourceQueryService
{
    private readonly Result<QueryExecutionResult> _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataSourceQueryService"/> class.
    /// </summary>
    /// <param name="result">The result to return.</param>
    public TestDataSourceQueryService(Result<QueryExecutionResult> result)
    {
        _result = result;
    }

    /// <summary>
    /// Returns the configured query result.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured result.</returns>
    public Task<Result<QueryExecutionResult>> ExecuteQueryAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string query,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_result);
    }
}