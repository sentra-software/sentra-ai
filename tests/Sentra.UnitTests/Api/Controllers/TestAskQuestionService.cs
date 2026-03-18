using Sentra.Application.Abstractions.AI;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.UnitTests.Api.Controllers;

/// <summary>
/// Represents a configurable test implementation of <see cref="IAskQuestionService"/>.
/// </summary>
internal sealed class TestAskQuestionService : IAskQuestionService
{
    private readonly Result<AskQuestionResult> _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAskQuestionService"/> class.
    /// </summary>
    /// <param name="result">The result to return.</param>
    public TestAskQuestionService(Result<AskQuestionResult> result)
    {
        _result = result;
    }

    /// <summary>
    /// Returns the configured ask-question result.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="question">The user question.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configured result.</returns>
    public Task<Result<AskQuestionResult>> AskAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_result);
    }
}