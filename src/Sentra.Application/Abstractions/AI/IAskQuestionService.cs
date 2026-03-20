using Sentra.Application.Abstractions.Auditing;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.AI;

/// <summary>
/// Defines a service for asking questions against a data source.
/// </summary>
public interface IAskQuestionService
{
    /// <summary>
    /// Processes a natural-language question and returns a structured response.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="question">The user question.</param>
    /// <param name="auditContext">Optional audit context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result containing the answer.</returns>
    Task<Result<AskQuestionResult>> AskAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        AskQuestionAuditContext? auditContext = null,
        CancellationToken cancellationToken = default
    );
}