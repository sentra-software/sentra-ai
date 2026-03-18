using System.Text;
using Sentra.AI.Abstractions.Answers;
using Sentra.AI.Abstractions.Chat;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.SharedKernel.Results;

namespace Sentra.AI.Orchestration.Answers;

/// <summary>
/// Represents the default answer generation service for Sentra.
/// </summary>
public sealed class AnswerGenerationService : IAnswerGenerationService
{
    private readonly IChatModelClient _chatModelClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnswerGenerationService"/> class.
    /// </summary>
    /// <param name="chatModelClient">The chat model client.</param>
    public AnswerGenerationService(IChatModelClient chatModelClient)
    {
        ArgumentNullException.ThrowIfNull(chatModelClient);

        _chatModelClient = chatModelClient;
    }

    /// <summary>
    /// Generates a natural-language answer for the provided question and query result.
    /// </summary>
    /// <param name="question">The original user question.</param>
    /// <param name="generatedSql">The generated SQL query.</param>
    /// <param name="queryResult">The executed query result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated answer,
    /// or a failed result describing the error.
    /// </returns>
    public async Task<Result<string>> GenerateAnswerAsync(
        string question,
        string generatedSql,
        QueryExecutionResult queryResult,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Failure<string>(
                Error.Validation(
                    "ai.answer.question.required",
                    "Question is required."));
        }

        if (string.IsNullOrWhiteSpace(generatedSql))
        {
            return Result.Failure<string>(
                Error.Validation(
                    "ai.answer.sql.required",
                    "Generated SQL is required."));
        }

        string? templateAnswer = TryGenerateTemplateAnswer(question, queryResult);
        if (templateAnswer is not null)
        {
            return Result.Success(templateAnswer);
        }

        string? rowsPreview = BuildRowsPreview(queryResult, 5);

        ChatMessage[]? messages = new[]
        {
            new ChatMessage(
                "system",
                """
                You are a business data assistant.
                Generate a concise, helpful answer based strictly on the provided SQL result.
                Do not invent facts.
                Do not mention technical implementation details unless useful.
                Keep the answer readable and direct.
                """),
            new ChatMessage(
                "user",
                $"""
                Question:
                {question}

                Generated SQL:
                {generatedSql}

                Row count:
                {queryResult.RowCount}

                Columns:
                {string.Join(", ", queryResult.Columns)}

                Rows preview:
                {rowsPreview}
                """)
        };

        Result<ChatCompletionResult>? completionResult = await _chatModelClient.CompleteAsync(messages, cancellationToken);
        if (completionResult.IsFailure)
        {
            return Result.Failure<string>(completionResult.Error);
        }

        string? answer = completionResult.ValueOrThrow().Content.Trim();

        if (string.IsNullOrWhiteSpace(answer))
        {
            return Result.Failure<string>(
                Error.Failure(
                    "ai.answer.invalid_response",
                    "The AI model returned an empty answer."));
        }

        return Result.Success(answer);
    }

    /// <summary>
    /// Attempts to generate a deterministic answer for common result shapes.
    /// </summary>
    /// <param name="question">The original user question.</param>
    /// <param name="queryResult">The query result.</param>
    /// <returns>A template answer when available; otherwise, <see langword="null"/>.</returns>
    private static string? TryGenerateTemplateAnswer(
        string question,
        QueryExecutionResult queryResult)
    {
        string? normalizedQuestion = question.Trim().ToLowerInvariant();

        if (queryResult.RowCount == 0)
        {
            return "I could not find any matching records for your question.";
        }

        if (queryResult.RowCount == 1 && queryResult.Rows.FirstOrDefault() is { } firstRow)
        {
            if (TryGetValueIgnoreCase(firstRow, "UserCount", out object? userCount))
            {
                return $"There are {userCount} users.";
            }

            if (TryGetValueIgnoreCase(firstRow, "CompanyCount", out object? companyCount))
            {
                return $"There are {companyCount} companies.";
            }

            if ((normalizedQuestion.Contains("latest companies", StringComparison.Ordinal) ||
                 normalizedQuestion.Contains("recent companies", StringComparison.Ordinal) ||
                 normalizedQuestion.Contains("latest company", StringComparison.Ordinal)) &&
                TryGetValueIgnoreCase(firstRow, "Name", out object? name))
            {
                if (TryGetValueIgnoreCase(firstRow, "CreatedAt", out object? createdAt))
                {
                    return $"The most recently created company is {name}, created at {createdAt}.";
                }

                return $"The most recently created company is {name}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Builds a small textual preview of query rows for AI summarization.
    /// </summary>
    /// <param name="queryResult">The query result.</param>
    /// <param name="maxRows">The maximum number of rows to include.</param>
    /// <returns>A textual preview of the rows.</returns>
    private static string BuildRowsPreview(
        QueryExecutionResult queryResult,
        int maxRows)
    {
        if (queryResult.Rows.Count == 0)
        {
            return "(no rows)";
        }

        StringBuilder? builder = new StringBuilder();
        IReadOnlyDictionary<string, object?>[]? rows = queryResult.Rows.Take(maxRows).ToArray();

        for (int index = 0; index < rows.Length; index++)
        {
            builder.Append("Row ")
                .Append(index + 1)
                .Append(": ");

            IEnumerable<string>? parts = rows[index]
                .Select(pair => $"{pair.Key}={pair.Value}");

            builder.Append(string.Join(", ", parts))
                .AppendLine();
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Attempts to read a value from a row dictionary using a case-insensitive key lookup.
    /// </summary>
    /// <param name="row">The row dictionary.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The resolved value when found.</param>
    /// <returns><see langword="true"/> when the value was found; otherwise, <see langword="false"/>.</returns>
    private static bool TryGetValueIgnoreCase(
        IReadOnlyDictionary<string, object?> row,
        string key,
        out object? value)
    {
        foreach (KeyValuePair<string, object?> pair in row)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}