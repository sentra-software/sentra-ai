using System.Diagnostics;
using Sentra.AI.Abstractions.Answers;
using Sentra.AI.Abstractions.Sql;
using Sentra.Application.Abstractions.AI;
using Sentra.Application.Abstractions.Auditing;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Querying;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.Auditing;
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
    private readonly IAiQueryAuditLogWriter _auditLogWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="AskQuestionService"/> class.
    /// </summary>
    public AskQuestionService(
        IDataSourceSchemaService schemaService,
        IDataSourceQueryService queryService,
        ISqlGenerationService sqlGenerationService,
        IQuerySafetyValidator querySafetyValidator,
        IAnswerGenerationService answerGenerationService,
        IAiQueryAuditLogWriter auditLogWriter)
    {
        ArgumentNullException.ThrowIfNull(schemaService);
        ArgumentNullException.ThrowIfNull(queryService);
        ArgumentNullException.ThrowIfNull(sqlGenerationService);
        ArgumentNullException.ThrowIfNull(querySafetyValidator);
        ArgumentNullException.ThrowIfNull(answerGenerationService);
        ArgumentNullException.ThrowIfNull(auditLogWriter);

        _schemaService = schemaService;
        _queryService = queryService;
        _sqlGenerationService = sqlGenerationService;
        _querySafetyValidator = querySafetyValidator;
        _answerGenerationService = answerGenerationService;
        _auditLogWriter = auditLogWriter;
    }

    /// <inheritdoc />
    public async Task<Result<AskQuestionResult>> AskAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        AskQuestionAuditContext? auditContext = null,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string? generatedSql = null;
        int? rowCount = null;

        async Task WriteAuditAsync(
            AiQueryAuditStatus status,
            string? errorCode = null,
            string? errorMessage = null)
        {
            if (auditContext is null)
            {
                return;
            }

            await _auditLogWriter.WriteAsync(
                new AiQueryAuditLogEntry(
                    auditContext.TenantId,
                    auditContext.IdentityUserId,
                    auditContext.DataSourceId,
                    question,
                    generatedSql,
                    rowCount,
                    status,
                    errorCode,
                    errorMessage,
                    (int)stopwatch.ElapsedMilliseconds),
                cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(
                Error.Validation("ai.connection_string.required", "Connection string is required."));
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(
                Error.Validation("ai.question.required", "Question is required."));
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        string trimmedQuestion = question.Trim();
        string trimmedConnectionString = connectionString.Trim();

        Result<IReadOnlyCollection<TableSchema>> schemaResult =
            await _schemaService.ReadSchemaAsync(dataSourceType, trimmedConnectionString, cancellationToken);

        if (schemaResult.IsFailure)
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(schemaResult.Error);
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        Result<string> sqlResult = await _sqlGenerationService.GenerateSqlAsync(
            dataSourceType,
            trimmedQuestion,
            schemaResult.ValueOrThrow(),
            cancellationToken);

        if (sqlResult.IsFailure)
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(sqlResult.Error);
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        generatedSql = sqlResult.ValueOrThrow();

        Result safetyResult = _querySafetyValidator.Validate(generatedSql);
        if (safetyResult.IsFailure)
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(safetyResult.Error);
            await WriteAuditAsync(AiQueryAuditStatus.Blocked, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        Result<QueryExecutionResult> queryResult = await _queryService.ExecuteQueryAsync(
            dataSourceType,
            trimmedConnectionString,
            generatedSql,
            cancellationToken);

        if (queryResult.IsFailure)
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(queryResult.Error);
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        QueryExecutionResult executedQueryResult = queryResult.ValueOrThrow();
        rowCount = executedQueryResult.RowCount;

        Result<string> answerResult = await _answerGenerationService.GenerateAnswerAsync(
            trimmedQuestion,
            generatedSql,
            executedQueryResult,
            cancellationToken);

        if (answerResult.IsFailure)
        {
            Result<AskQuestionResult> failure = Result.Failure<AskQuestionResult>(answerResult.Error);
            await WriteAuditAsync(AiQueryAuditStatus.Failed, failure.Error.Code, failure.Error.Message);
            return failure;
        }

        AskQuestionResult successValue = new(
            trimmedQuestion,
            generatedSql,
            executedQueryResult,
            answerResult.ValueOrThrow());

        await WriteAuditAsync(AiQueryAuditStatus.Succeeded);

        return Result.Success(successValue);
    }
}