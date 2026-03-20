using System.Text.RegularExpressions;
using Sentra.Application.Abstractions.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.DataSources;

/// <summary>
/// Represents the default SQL query safety validator for Sentra.
/// </summary>
public sealed partial class QuerySafetyValidator : IQuerySafetyValidator
{
    private static readonly string[] ForbiddenKeywords =
    [
        "INSERT",
        "UPDATE",
        "DELETE",
        "DROP",
        "ALTER",
        "TRUNCATE",
        "CREATE",
        "GRANT",
        "REVOKE",
        "MERGE",
        "CALL",
        "EXEC",
        "EXECUTE",
        "ATTACH",
        "DETACH",
        "COPY",
        "REPLACE",
        "VACUUM",
        "ANALYZE",
        "PRAGMA",
        "INTO"
    ];

    /// <inheritdoc/>
    public Result Validate(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result.Failure(
                Error.Validation(
                    "datasources.query.required",
                    "Query is required."));
        }

        string? trimmedQuery = query.Trim();

        if (!trimmedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            (!trimmedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure(
                Error.Validation(
                    "datasources.query.only_select_allowed",
                    "Only SELECT queries are allowed."));
        }

        if (ContainsSqlComment(trimmedQuery))
        {
            return Result.Failure(
                Error.Validation(
                    "datasources.query.comments_not_allowed",
                    "SQL comments are not allowed."));
        }

        if (ContainsMultipleStatements(trimmedQuery))
        {
            return Result.Failure(
                Error.Validation(
                    "datasources.query.multiple_statements_not_allowed",
                    "Only a single SQL statement is allowed."));
        }

        foreach (string? forbiddenKeyword in ForbiddenKeywords)
        {
            if (ContainsKeyword(trimmedQuery, forbiddenKeyword))
            {
                return Result.Failure(
                    Error.Validation(
                        "datasources.query.forbidden_keyword",
                        $"The SQL keyword '{forbiddenKeyword}' is not allowed."));
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Determines whether the query contains SQL comments.
    /// </summary>
    /// <param name="query">The query to inspect.</param>
    /// <returns><see langword="true"/> when comments are detected; otherwise, <see langword="false"/>.</returns>
    private static bool ContainsSqlComment(string query)
    {
        return query.Contains("--", StringComparison.Ordinal) ||
               query.Contains("/*", StringComparison.Ordinal) ||
               query.Contains("*/", StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the query contains multiple SQL statements.
    /// </summary>
    /// <param name="query">The query to inspect.</param>
    /// <returns><see langword="true"/> when multiple statements are detected; otherwise, <see langword="false"/>.</returns>
    private static bool ContainsMultipleStatements(string query)
    {
        int semicolonCount = query.Count(static character => character == ';');

        if (semicolonCount == 0)
        {
            return false;
        }

        if (semicolonCount > 1)
        {
            return true;
        }

        return query[^1] != ';';
    }

    /// <summary>
    /// Determines whether the query contains the specified SQL keyword as a whole word.
    /// </summary>
    /// <param name="query">The query to inspect.</param>
    /// <param name="keyword">The keyword to look for.</param>
    /// <returns><see langword="true"/> when the keyword is found; otherwise, <see langword="false"/>.</returns>
    private static bool ContainsKeyword(string query, string keyword)
    {
        return KeywordRegex(keyword).IsMatch(query);
    }

    /// <summary>
    /// Creates a compiled regular expression that matches a keyword as a whole word.
    /// </summary>
    /// <returns>The compiled regular expression.</returns>
    [GeneratedRegex(@"\bPLACEHOLDER\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();

    private static Regex KeywordRegex(string keyword)
    {
        string? pattern = $@"\b{Regex.Escape(keyword)}\b";
        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    }
}