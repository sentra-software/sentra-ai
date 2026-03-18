using Sentra.SharedKernel.Results;

namespace Sentra.Application.Abstractions.DataSources;

/// <summary>
/// Defines a validator that determines whether a SQL query is safe to execute.
/// </summary>
public interface IQuerySafetyValidator
{
    /// <summary>
    /// Validates whether the provided SQL query is considered safe for execution.
    /// </summary>
    /// <param name="query">The raw SQL query.</param>
    /// <returns>
    /// A successfull result when the query is considered safe; otherwise, a failed result
    /// containing the validation error.
    /// </returns>
    Result Validate(string query);
}