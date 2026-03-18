using System.Text;
using Sentra.AI.Abstractions.Chat;
using Sentra.AI.Abstractions.Sql;
using Sentra.Connectors.Abstractions.Schema;
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
    /// <param name="question">The user question.</param>
    /// <param name="tables">The available schema tables.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated SQL query,
    /// or a failed result describing the error.
    /// </returns>
    public async Task<Result<string>> GenerateSqlAsync(
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

        SqlTemplateMatchResult? templateMatchResult = _sqlTemplateMatcher.Match(question, tables);
        if (templateMatchResult.IsMatch && !string.IsNullOrWhiteSpace(templateMatchResult.Sql))
        {
            return Result.Success(templateMatchResult.Sql);
        }

        string? schemaText = BuildSchemaText(tables);

        ChatMessage[]? messages = new[]
        {
            new ChatMessage(
                "system",
                """
                You are a PostgreSQL SQL generation assistant.

                Rules:
                - Generate exactly one read-only SQL SELECT statement.
                - Never generate INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, CREATE, GRANT, REVOKE, MERGE, CALL, EXEC, or EXECUTE.
                - Never generate comments.
                - Never generate markdown.
                - Never wrap the SQL in code fences.
                - Use exact table names and column names from the provided schema.
                - Use PostgreSQL syntax.
                - Always quote table names and column names with double quotes when using schema-defined identifiers.
                - Never invent tables or columns.
                - Prefer ORDER BY ... DESC LIMIT ... for questions about latest or recent records.
                - Return only SQL.
                """),
            new ChatMessage(
                "user",
                $"""
                Schema:
                {schemaText}

                Question:
                {question}
                """)
        };

        Result<ChatCompletionResult>? completionResult = await _chatModelClient.CompleteAsync(messages, cancellationToken);
        if (completionResult.IsFailure)
        {
            return Result.Failure<string>(completionResult.Error);
        }

        string? generatedSql = completionResult.ValueOrThrow().Content.Trim();
        string? normalizedSql = _sqlIdentifierNormalizer.Normalize(generatedSql, tables);

        return Result.Success(normalizedSql);
    }

    /// <summary>
    /// Builds a compact textual representation of the schema.
    /// </summary>
    /// <param name="tables">The tables.</param>
    /// <returns>The formatted schema text.</returns>
    private static string BuildSchemaText(IReadOnlyCollection<TableSchema> tables)
    {
        StringBuilder? builder = new StringBuilder();

        foreach (TableSchema? table in tables.OrderBy(table => table.Schema).ThenBy(table => table.Name))
        {
            builder.Append("Table: ")
                .Append(table.Schema)
                .Append(".\"")
                .Append(table.Name)
                .Append('"')
                .AppendLine();

            foreach (ColumnSchema? column in table.Columns)
            {
                builder.Append("  - \"")
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
}