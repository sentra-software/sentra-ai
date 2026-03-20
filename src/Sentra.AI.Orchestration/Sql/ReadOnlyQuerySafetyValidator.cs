using System.Text.RegularExpressions;
using Sentra.Application.Abstractions.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Orchestration.Sql;

/// <summary>
/// Validates that generated SQL is read-only and single-statement.
/// </summary>
public sealed class ReadOnlyQuerySafetyValidator : IQuerySafetyValidator
{
    private static readonly string[] ForbiddenKeywords =
    [
        "insert",
        "update",
        "delete",
        "drop",
        "alter",
        "truncate",
        "create",
        "grant",
        "revoke",
        "merge",
        "attach",
        "detach",
        "copy",
        "call",
        "execute",
        "exec",
        "replace",
        "vacuum",
        "reindex",
        "analyze"
    ];

    /// <inheritdoc />
    public Result Validate(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result.Failure(Error.Validation(
                "query_guard.query.required",
                "Generated SQL is required."));
        }

        string trimmed = query.Trim();

        if (trimmed.Contains(';'))
        {
            return Result.Failure(Error.Validation(
                "query_guard.multiple_statements.blocked",
                "Multiple SQL statements are not allowed."));
        }

        string normalized = Regex.Replace(trimmed, @"\s+", " ").Trim().ToLowerInvariant();

        bool startsReadOnly =
            normalized.StartsWith("select ") ||
            normalized.StartsWith("select\n") ||
            normalized.StartsWith("with ");

        if (!startsReadOnly)
        {
            return Result.Failure(Error.Validation(
                "query_guard.read_only_required",
                "Only read-only SELECT or WITH queries are allowed."));
        }

        foreach (string keyword in ForbiddenKeywords)
        {
            if (Regex.IsMatch(normalized, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
            {
                return Result.Failure(Error.Validation(
                    "query_guard.keyword.blocked",
                    $"The generated SQL contains a blocked keyword: {keyword}."));
            }
        }

        return Result.Success();
    }
}