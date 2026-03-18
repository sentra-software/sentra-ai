using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Represents a configurable test implementation of <see cref="IDataSourceConnectionService"/>.
/// </summary>
internal sealed class TestDataSourceConnectionService : IDataSourceConnectionService
{
    private readonly Result<ConnectionTestResult> _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataSourceConnectionService"/> class.
    /// </summary>
    /// <param name="result">The result to return.</param>
    public TestDataSourceConnectionService(Result<ConnectionTestResult> result)
    {
        _result = result;
    }

    /// <summary>
    /// Returns the configured connection test result.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured result.</returns>
    public Task<Result<ConnectionTestResult>> TestConnectionAsync(
        DataSourceType dataSourceType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_result);
    }
}