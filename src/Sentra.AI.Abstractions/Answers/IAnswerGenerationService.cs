using Sentra.Connectors.Abstractions.Querying;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Abstractions.Answers;

/// <summary>
/// Defines a service for generating a natural-language answer from a question,
/// executed SQL, and query result data.
/// </summary>
public interface IAnswerGenerationService
{
    /// <summary>
    /// Generates a natural-language answer for the provided question and query result.
    /// </summary>
    /// <param name="question">The original user question.</param>
    /// <param name="generatedSql">The generated SQL query.</param>
    /// <param name="queryResult">The executed query result/</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated answer,
    /// or a failed result describing the error.
    /// </returns>
    Task<Result<string>> GenerateAnswerAsync(
        string question,
        string generatedSql,
        QueryExecutionResult queryResult,
        CancellationToken cancellationToken = default
    );
}