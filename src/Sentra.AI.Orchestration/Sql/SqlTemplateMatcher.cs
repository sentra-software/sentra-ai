using Sentra.AI.Abstractions.Sql;
using Sentra.Connectors.Abstractions.Schema;

namespace Sentra.AI.Orchestration.Sql;

/// <summary>
/// Represents a simple rule-based SQL template matcher for common Sentra MVP questions.
/// </summary>
public sealed class SqlTemplateMatcher : ISqlTemplateMatcher
{
    /// <summary>
    /// Attempts to match the provided question to a predefined SQL template using the available schema.
    /// </summary>
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <returns>
    /// A <see cref="SqlTemplateMatchResult"/> indicating whether a template match was found.
    /// </returns>
    public SqlTemplateMatchResult Match(
        string question,
        IReadOnlyCollection<TableSchema> tables)
    {
        if (string.IsNullOrWhiteSpace(question) || tables is null || tables.Count == 0)
        {
            return SqlTemplateMatchResult.NoMatch();
        }

        string? normalizedQuestion = question.Trim().ToLowerInvariant();

        if (ContainsAll(normalizedQuestion, "latest", "companies") ||
            ContainsAll(normalizedQuestion, "recent", "companies"))
        {
            TableSchema? table = FindTable(tables, "Companies");
            if (table is not null)
            {
                return SqlTemplateMatchResult.Match($"""
                    select "Id", "Name", "CreatedAt", "OwnerUserId"
                    from {FormatQualifiedTableName(table)}
                    order by "CreatedAt" desc
                    limit 10;
                    """);
            }
        }

        if (ContainsAll(normalizedQuestion, "latest", "logs") ||
            ContainsAll(normalizedQuestion, "recent", "logs"))
        {
            if (ContainsAll(normalizedQuestion, "audit", "logs"))
            {
                TableSchema? auditLogsTable = FindTable(tables, "PlatformAuditLogs");
                if (auditLogsTable is not null)
                {
                    return SqlTemplateMatchResult.Match($"""
                        select "Id", "Timestamp", "Category", "Level", "Action", "Message"
                        from {FormatQualifiedTableName(auditLogsTable)}
                        order by "Timestamp" desc
                        limit 20;
                        """);
                }
            }

            if (ContainsAll(normalizedQuestion, "performance", "logs"))
            {
                TableSchema? performanceLogsTable = FindTable(tables, "PlatformPerformanceLogs");
                if (performanceLogsTable is not null)
                {
                    return SqlTemplateMatchResult.Match($"""
                        select "Id", "Timestamp", "Source", "Method", "Path", "DurationMs", "Success"
                        from {FormatQualifiedTableName(performanceLogsTable)}
                        order by "Timestamp" desc
                        limit 20;
                        """);
                }
            }

            TableSchema? companyLogsTable = FindTable(tables, "CompanyLogs");
            if (companyLogsTable is not null)
            {
                return SqlTemplateMatchResult.Match($"""
                    select "Id", "Timestamp", "EventType", "ModuleKey", "Message", "GuildId"
                    from {FormatQualifiedTableName(companyLogsTable)}
                    order by "Timestamp" desc
                    limit 20;
                    """);
            }
        }

        if ((normalizedQuestion.Contains("how many users", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("count users", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("number of users", StringComparison.Ordinal)) &&
            FindTable(tables, "Users") is { } usersTable)
        {
            return SqlTemplateMatchResult.Match($"""
                select count(*) as "UserCount"
                from {FormatQualifiedTableName(usersTable)};
                """);
        }

        if ((normalizedQuestion.Contains("how many companies", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("count companies", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("number of companies", StringComparison.Ordinal)) &&
            FindTable(tables, "Companies") is { } companiesTable)
        {
            return SqlTemplateMatchResult.Match($"""
                select count(*) as "CompanyCount"
                from {FormatQualifiedTableName(companiesTable)};
                """);
        }

        return SqlTemplateMatchResult.NoMatch();
    }

    /// <summary>
    /// Determines whether the provided question contains all specified tokens.
    /// </summary>
    /// <param name="question">The normalized question.</param>
    /// <param name="tokens">The tokens to check.</param>
    /// <returns><see langword="true"/> when all tokens are present; otherwise, <see langword="false"/>.</returns>
    private static bool ContainsAll(string question, params string[] tokens)
    {
        return tokens.All(token => question.Contains(token, StringComparison.Ordinal));
    }

    /// <summary>
    /// Finds a table in the schema by exact table name, ignoring case.
    /// </summary>
    /// <param name="tables">The available tables.</param>
    /// <param name="name">The table name to find.</param>
    /// <returns>The matching table when found; otherwise, <see langword="null"/>.</returns>
    private static TableSchema? FindTable(
        IReadOnlyCollection<TableSchema> tables,
        string name)
    {
        return tables.FirstOrDefault(table =>
            string.Equals(table.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Formats a qualified PostgreSQL table name using exact schema and table casing.
    /// </summary>
    /// <param name="table">The table schema metadata.</param>
    /// <returns>The formatted qualified table name.</returns>
    private static string FormatQualifiedTableName(TableSchema table)
    {
        return $@"{table.Schema}.""{table.Name}""";
    }
}