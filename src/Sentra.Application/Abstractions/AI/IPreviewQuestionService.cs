using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.AI;

/// <summary>
/// Defines a service for previewing generated SQL without execution.
/// </summary>
public interface IPreviewQuestionService
{
    /// <summary>
    /// Generates a safe SQL preview for the specified question.
    /// </summary>
    Task<Result<string>> PreviewAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        CancellationToken cancellationToken = default);
}