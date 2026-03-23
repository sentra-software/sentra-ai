using System.Text;
using Sentra.AI.Abstractions.Chat;
using Sentra.AI.Abstractions.Sql;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Orchestration.Sql;

/// <summary>
/// Represents the default SQL generation service.
/// </summary>
public sealed class SqlGenerationService : ISqlGenerationService
{
    private readonly IChatModelClient _chatModelClient;
    private readonly ISqlTemplateMatcher _sqlTemplateMatcher;
    private readonly ISqlIdentifierNormalizer _sqlIdentifierNormalizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlGenerationService"/> class.
    /// </summary>
    /// <param name="chatModelClient">The chat model client.</param>
    /// <param name="sqlTemplateMatcher">The SQL template matcher.</param>
    /// <param name="sqlIdentifierNormalizer">The SQL identifier normalizer.</param>
    public SqlGenerationService(
        IChatModelClient chatModelClient,
        ISqlTemplateMatcher sqlTemplateMatcher,
        ISqlIdentifierNormalizer sqlIdentifierNormalizer)
    {
        ArgumentNullException.ThrowIfNull(chatModelClient);
        ArgumentNullException.ThrowIfNull(sqlTemplateMatcher);
        ArgumentNullException.ThrowIfNull(sqlIdentifierNormalizer);

        _chatModelClient = chatModelClient;
        _sqlTemplateMatcher = sqlTemplateMatcher;
        _sqlIdentifierNormalizer = sqlIdentifierNormalizer;
    }

    /// <summary>
    /// Generates a SQL query for the provided question and schema.
    /// </summary>
    /// <param name="dataSourceType">The target data source type.</param>
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated SQL query,
    /// or a failed result describing the error.
    /// </returns>
    public async Task<Result<string>> GenerateSqlAsync(
        DataSourceType dataSourceType,
        string question,
        IReadOnlyCollection<TableSchema> tables,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Failure<string>(
                Error.Validation(
                    "ai.sql.question.required",
                    "Question is required."));
        }

        if (tables is null || tables.Count == 0)
        {
            return Result.Failure<string>(
                Error.Validation(
                    "ai.sql.schema.required",
                    "At least one table schema is required."));
        }

        SqlTemplateMatchResult templateMatchResult = _sqlTemplateMatcher.Match(
            dataSourceType,
            question,
            tables);

        if (templateMatchResult.IsMatch && !string.IsNullOrWhiteSpace(templateMatchResult.Sql))
        {
            return Result.Success(templateMatchResult.Sql);
        }

        string schemaText = BuildSchemaText(tables);
        string dialectName = GetDialectName(dataSourceType);
        string identifierRule = GetIdentifierRule(dataSourceType);
        string syntaxRules = GetSyntaxRules(dataSourceType);

        ChatMessage[] messages =
        [
            new ChatMessage(
                "system",
                $"""
                You are a {dialectName} SQL generation assistant.

                Rules:
                - Generate exactly one read-only SQL SELECT statement.
                - Never generate INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, CREATE, GRANT, REVOKE, MERGE, CALL, EXEC, or EXECUTE.
                - Never generate comments.
                - Never generate markdown.
                - Never wrap the SQL in code fences.
                - Use exact table names and column names from the provided schema.
                - Use {dialectName} syntax only.
                - {identifierRule}
                - Never invent tables or columns.
                - Prefer ORDER BY ... DESC LIMIT ... for questions about latest or recent records.
                - Return only SQL.

                Dialect-specific notes:
                {syntaxRules}
                """),
            new ChatMessage(
                "user",
                $"""
                Schema:
                {schemaText}

                Question:
                {question}
                """)
        ];

        Result<ChatCompletionResult>? completionResult = await _chatModelClient.CompleteAsync(
            messages,
            cancellationToken);

        if (completionResult.IsFailure)
        {
            return Result.Failure<string>(completionResult.Error);
        }

        string generatedSql = completionResult.ValueOrThrow().Content.Trim();

        string normalizedSql = _sqlIdentifierNormalizer.Normalize(
            dataSourceType,
            generatedSql,
            tables);

        return Result.Success(normalizedSql);
    }

    /// <summary>
    /// Builds a compact textual representation of the schema.
    /// </summary>
    /// <param name="tables">The tables.</param>
    /// <returns>The formatted schema text.</returns>
    private static string BuildSchemaText(IReadOnlyCollection<TableSchema> tables)
    {
        StringBuilder builder = new();

        foreach (TableSchema table in tables.OrderBy(table => table.Schema).ThenBy(table => table.Name))
        {
            builder.Append("Table: ")
                .Append(table.Schema)
                .Append('.')
                .Append('"')
                .Append(table.Name)
                .Append('"')
                .AppendLine();

            foreach (ColumnSchema column in table.Columns)
            {
                builder.Append(" - \"")
                    .Append(column.Name)
                    .Append("\" : ")
                    .Append(column.DataType)
                    .Append(" nullable=")
                    .Append(column.IsNullable)
                    .AppendLine();
            }
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Gets the SQL dialect display name for the specified data source type.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <returns>The SQL dialect display name.</returns>
    private static string GetDialectName(DataSourceType dataSourceType)
    {
        return dataSourceType switch
        {
            DataSourceType.PostgreSql => "PostgreSQL",
            DataSourceType.MySql => "MySQL",
            DataSourceType.Sqlite => "SQLite",
            DataSourceType.SqlServer => "SQL Server",
            _ => "SQL"
        };
    }

    /// <summary>
    /// Gets the identifier quoting rule for the specified dialect.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <returns>The identifier quoting rule.</returns>
    private static string GetIdentifierRule(DataSourceType dataSourceType)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql =>
                "Always quote schema, table, and column names with backticks when using schema-defined identifiers.",
            DataSourceType.Sqlite =>
                "Prefer double quotes for identifiers when quoting is needed, and do not invent schemas beyond the provided schema metadata.",
            DataSourceType.SqlServer =>
                "Prefer square brackets for identifiers when quoting is needed.",
            _ =>
                "Always quote schema, table, and column names with double quotes when using schema-defined identifiers."
        };
    }

    /// <summary>
    /// Gets additional dialect-specific prompt rules.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <returns>The dialect-specific rules.</returns>
    private static string GetSyntaxRules(DataSourceType dataSourceType)
    {
        return dataSourceType switch
        {
            DataSourceType.MySql =>
                """
                - Use backticks for quoted identifiers.
                - Use LIMIT for row limiting.
                - Avoid PostgreSQL-specific casts and operators.
                - Do not use ILIKE; prefer LIKE unless case-insensitive behavior is clearly required and supported otherwise.
                """,
            DataSourceType.Sqlite =>
                """
                - SQLite typically uses the main schema.
                - Use LIMIT for row limiting.
                - Avoid PostgreSQL-specific casts, operators, and information_schema assumptions.
                - Prefer simple ANSI-compatible SQL where possible.
                """,
            DataSourceType.SqlServer =>
                """
                - Use SQL Server syntax only.
                - Prefer TOP (...) for row limiting instead of LIMIT.
                - Use square brackets for quoted identifiers when needed.
                """,
            _ =>
                """
                - Use PostgreSQL syntax only.
                - Use double quotes for quoted identifiers.
                - Use LIMIT for row limiting.
                """
        };
    }
}