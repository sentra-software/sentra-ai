using Sentra.AI.Abstractions.Sql;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;

namespace Sentra.AI.Orchestration.Sql;

/// <summary>
/// Represents a simple rule-based SQL template matcher for common Sentra MVP questions.
/// </summary>
public sealed class SqlTemplateMatcher : ISqlTemplateMatcher
{
    /// <summary>
    /// Attempts to match the provided question to a predefined SQL template using the available schema.
    /// </summary>
    /// <param name="dataSourceType">The target data source type.</param>
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <returns>
    /// A <see cref="SqlTemplateMatchResult"/> indicating whether a template match was found.
    /// </returns>
    public SqlTemplateMatchResult Match(
        DataSourceType dataSourceType,
        string question,
        IReadOnlyCollection<TableSchema> tables)
    {
        if (string.IsNullOrWhiteSpace(question) || tables is null || tables.Count == 0)
        {
            return SqlTemplateMatchResult.NoMatch();
        }

        string normalizedQuestion = question.Trim().ToLowerInvariant();

        if (ContainsAll(normalizedQuestion, "latest", "companies") ||
            ContainsAll(normalizedQuestion, "recent", "companies"))
        {
            TableSchema? table = FindTable(tables, "Companies");

            if (table is not null)
            {
                return SqlTemplateMatchResult.Match(
                    $"""
                    select {FormatColumn(dataSourceType, "Id")}, {FormatColumn(dataSourceType, "Name")}, {FormatColumn(dataSourceType, "CreatedAt")}, {FormatColumn(dataSourceType, "OwnerUserId")}
                    from {FormatQualifiedTableName(dataSourceType, table)}
                    order by {FormatColumn(dataSourceType, "CreatedAt")} desc
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
                    return SqlTemplateMatchResult.Match(
                        $"""
                        select {FormatColumn(dataSourceType, "Id")}, {FormatColumn(dataSourceType, "Timestamp")}, {FormatColumn(dataSourceType, "Category")}, {FormatColumn(dataSourceType, "Level")}, {FormatColumn(dataSourceType, "Action")}, {FormatColumn(dataSourceType, "Message")}
                        from {FormatQualifiedTableName(dataSourceType, auditLogsTable)}
                        order by {FormatColumn(dataSourceType, "Timestamp")} desc
                        limit 20;
                        """);
                }
            }

            if (ContainsAll(normalizedQuestion, "performance", "logs"))
            {
                TableSchema? performanceLogsTable = FindTable(tables, "PlatformPerformanceLogs");

                if (performanceLogsTable is not null)
                {
                    return SqlTemplateMatchResult.Match(
                        $"""
                        select {FormatColumn(dataSourceType, "Id")}, {FormatColumn(dataSourceType, "Timestamp")}, {FormatColumn(dataSourceType, "Source")}, {FormatColumn(dataSourceType, "Method")}, {FormatColumn(dataSourceType, "Path")}, {FormatColumn(dataSourceType, "DurationMs")}, {FormatColumn(dataSourceType, "Success")}
                        from {FormatQualifiedTableName(dataSourceType, performanceLogsTable)}
                        order by {FormatColumn(dataSourceType, "Timestamp")} desc
                        limit 20;
                        """);
                }
            }

            TableSchema? companyLogsTable = FindTable(tables, "CompanyLogs");

            if (companyLogsTable is not null)
            {
                return SqlTemplateMatchResult.Match(
                    $"""
                    select {FormatColumn(dataSourceType, "Id")}, {FormatColumn(dataSourceType, "Timestamp")}, {FormatColumn(dataSourceType, "EventType")}, {FormatColumn(dataSourceType, "ModuleKey")}, {FormatColumn(dataSourceType, "Message")}, {FormatColumn(dataSourceType, "GuildId")}
                    from {FormatQualifiedTableName(dataSourceType, companyLogsTable)}
                    order by {FormatColumn(dataSourceType, "Timestamp")} desc
                    limit 20;
                    """);
            }
        }

        if ((normalizedQuestion.Contains("how many users", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("count users", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("number of users", StringComparison.Ordinal)) &&
            FindTable(tables, "Users") is { } usersTable)
        {
            return SqlTemplateMatchResult.Match(
                $"""
                select count(*) as {FormatAlias(dataSourceType, "UserCount")}
                from {FormatQualifiedTableName(dataSourceType, usersTable)};
                """);
        }

        if ((normalizedQuestion.Contains("how many companies", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("count companies", StringComparison.Ordinal) ||
             normalizedQuestion.Contains("number of companies", StringComparison.Ordinal)) &&
            FindTable(tables, "Companies") is { } companiesTable)
        {
            return SqlTemplateMatchResult.Match(
                $"""
                select count(*) as {FormatAlias(dataSourceType, "CompanyCount")}
                from {FormatQualifiedTableName(dataSourceType, companiesTable)};
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
    /// Formats a qualified table name for the selected dialect.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="table">The table schema metadata.</param>
    /// <returns>The formatted qualified table name.</returns>
    private static string FormatQualifiedTableName(DataSourceType dataSourceType, TableSchema table)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql => $"`{table.Schema}`.`{table.Name}`",
            DataSourceType.Sqlite => $"\"{table.Name}\"",
            DataSourceType.SqlServer => $"[{table.Schema}].[{table.Name}]",
            _ => $@"{table.Schema}.""{table.Name}"""
        };
    }

    /// <summary>
    /// Formats a column identifier for the selected dialect.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>The formatted column identifier.</returns>
    private static string FormatColumn(DataSourceType dataSourceType, string columnName)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql => $"`{columnName}`",
            DataSourceType.SqlServer => $"[{columnName}]",
            _ => $"\"{columnName}\""
        };
    }

    /// <summary>
    /// Formats an alias for the selected dialect.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="alias">The alias.</param>
    /// <returns>The formatted alias.</returns>
    private static string FormatAlias(DataSourceType dataSourceType, string alias)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql => $"`{alias}`",
            DataSourceType.SqlServer => $"[{alias}]",
            _ => $"\"{alias}\""
        };
    }
}