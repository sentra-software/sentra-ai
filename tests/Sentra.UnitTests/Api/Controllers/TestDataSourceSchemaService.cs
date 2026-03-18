using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Represents a configurable test implementation of <see cref="IDataSourceSchemaService"/>.
/// </summary>
internal sealed class TestDataSourceSchemaService : IDataSourceSchemaService
{
    private readonly Result<IReadOnlyCollection<TableSchema>> _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataSourceSchemaService"/> class.
    /// </summary>
    /// <param name="result">The result to return.</param>
    public TestDataSourceSchemaService(Result<IReadOnlyCollection<TableSchema>> result)
    {
        _result = result;
    }

    /// <summary>
    /// Returns the configured schema result.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured result.</returns>
    public Task<Result<IReadOnlyCollection<TableSchema>>> ReadSchemaAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_result);
    }
}