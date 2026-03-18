using Sentra.Connectors.Abstractions.Schema;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Abstractions.Sql;

/// <summary>
/// Defines a service for generating SQL from a natural-language question and schema context.
/// </summary>
public interface ISqlGenerationService
{
    /// <summary>
    /// Generates a SQL query for the provided question and schema.
    /// </summary>
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated SQL query,
    /// or a failed result describing the error.
    /// </returns>
    Task<Result<string>> GenerateSqlAsync(
        string question,
        IReadOnlyCollection<TableSchema> tables,
        CancellationToken cancellationToken = default);
}