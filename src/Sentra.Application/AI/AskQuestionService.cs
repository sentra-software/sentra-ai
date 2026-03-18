using Sentra.AI.Abstractions.Answers;
using Sentra.AI.Abstractions.Sql;
using Sentra.Application.Abstractions.AI;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.AI;

/// <summary>
/// Represents the default implementation of <see cref="IAskQuestionService"/>.
/// </summary>
public sealed class AskQuestionService : IAskQuestionService
{
    private readonly IDataSourceSchemaService _schemaService;
    private readonly IDataSourceQueryService _queryService;
    private readonly ISqlGenerationService _sqlGenerationService;
    private readonly IQuerySafetyValidator _querySafetyValidator;
    private readonly IAnswerGenerationService _answerGenerationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AskQuestionService"/> class.
    /// </summary>
    /// <param name="schemaService">The schema service.</param>
    /// <param name="queryService">The query service.</param>
    /// <param name="sqlGenerationService">The SQL generation service.</param>
    /// <param name="querySafetyValidator">The query safety validator.</param>
    /// <param name="answerGenerationService">The answer generation service.</param>
    public AskQuestionService(
        IDataSourceSchemaService schemaService,
        IDataSourceQueryService queryService,
        ISqlGenerationService sqlGenerationService,
        IQuerySafetyValidator querySafetyValidator,
        IAnswerGenerationService answerGenerationService)
    {
        ArgumentNullException.ThrowIfNull(schemaService);
        ArgumentNullException.ThrowIfNull(queryService);
        ArgumentNullException.ThrowIfNull(sqlGenerationService);
        ArgumentNullException.ThrowIfNull(querySafetyValidator);
        ArgumentNullException.ThrowIfNull(answerGenerationService);

        _schemaService = schemaService;
        _queryService = queryService;
        _sqlGenerationService = sqlGenerationService;
        _querySafetyValidator = querySafetyValidator;
        _answerGenerationService = answerGenerationService;
    }

    /// <summary>
    /// Processes a natural-language question and returns a structured result.
    /// </summary>
    /// <param name="dataSourceType">The data source type.</param>
    /// <param name="connectionString">The raw connection string.</param>
    /// <param name="question">The user question.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A successful result containing the generated SQL, query result, and answer;
    /// otherwise, a failed result describing the error.
    /// </returns>
    public async Task<Result<AskQuestionResult>> AskAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Result.Failure<AskQuestionResult>(
                Error.Validation(
                    "ai.connection_string.required",
                    "Connection string is required."));
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Failure<AskQuestionResult>(
                Error.Validation(
                    "ai.question.required",
                    "Question is required."));
        }

        string? trimmedQuestion = question.Trim();
        string? trimmedConnectionString = connectionString.Trim();

        Result<IReadOnlyCollection<TableSchema>>? schemaResult = await _schemaService.ReadSchemaAsync(
            dataSourceType,
            trimmedConnectionString,
            cancellationToken);

        if (schemaResult.IsFailure)
        {
            return Result.Failure<AskQuestionResult>(schemaResult.Error);
        }

        Result<string>? sqlResult = await _sqlGenerationService.GenerateSqlAsync(
            trimmedQuestion,
            schemaResult.ValueOrThrow(),
            cancellationToken);

        if (sqlResult.IsFailure)
        {
            return Result.Failure<AskQuestionResult>(sqlResult.Error);
        }

        string? generatedSql = sqlResult.ValueOrThrow();

        Result? querySafetyResult = _querySafetyValidator.Validate(generatedSql);
        if (querySafetyResult.IsFailure)
        {
            return Result.Failure<AskQuestionResult>(querySafetyResult.Error);
        }

        Result<QueryExecutionResult>? queryResult = await _queryService.ExecuteQueryAsync(
            dataSourceType,
            trimmedConnectionString,
            generatedSql,
            cancellationToken);

        if (queryResult.IsFailure)
        {
            return Result.Failure<AskQuestionResult>(queryResult.Error);
        }

        QueryExecutionResult? executedQueryResult = queryResult.ValueOrThrow();

        Result<string>? answerResult = await _answerGenerationService.GenerateAnswerAsync(
            trimmedQuestion,
            generatedSql,
            executedQueryResult,
            cancellationToken);

        if (answerResult.IsFailure)
        {
            return Result.Failure<AskQuestionResult>(answerResult.Error);
        }

        return Result.Success(new AskQuestionResult(
            trimmedQuestion,
            generatedSql,
            executedQueryResult,
            answerResult.ValueOrThrow()));
    }
}