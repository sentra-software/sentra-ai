using Sentra.AI.Abstractions.Sql;
using Sentra.Application.Abstractions.AI;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Connectors.Abstractions.Schema;
using Sentra.Domain.DataSources;
using Sentra.SharedKernel.Results;

namespace Sentra.Application.AI;

/// <summary>
/// Generates SQL previews without executing them.
/// </summary>
public sealed class PreviewQuestionService : IPreviewQuestionService
{
    private readonly IDataSourceSchemaService _schemaService;
    private readonly ISqlGenerationService _sqlGenerationService;
    private readonly IQuerySafetyValidator _querySafetyValidator;

    public PreviewQuestionService(
        IDataSourceSchemaService schemaService,
        ISqlGenerationService sqlGenerationService,
        IQuerySafetyValidator querySafetyValidator)
    {
        _schemaService = schemaService;
        _sqlGenerationService = sqlGenerationService;
        _querySafetyValidator = querySafetyValidator;
    }

    public async Task<Result<string>> PreviewAsync(
        DataSourceType dataSourceType,
        string connectionString,
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Result.Failure<string>(Error.Validation(
                "ai.connection_string.required",
                "Connection string is required."));
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Failure<string>(Error.Validation(
                "ai.question.required",
                "Question is required."));
        }

        Result<IReadOnlyCollection<TableSchema>> schemaResult =
            await _schemaService.ReadSchemaAsync(dataSourceType, connectionString.Trim(), cancellationToken);

        if (schemaResult.IsFailure)
        {
            return Result.Failure<string>(schemaResult.Error);
        }

        Result<string> sqlResult = await _sqlGenerationService.GenerateSqlAsync(
            question.Trim(),
            schemaResult.ValueOrThrow(),
            cancellationToken);

        if (sqlResult.IsFailure)
        {
            return Result.Failure<string>(sqlResult.Error);
        }

        string generatedSql = sqlResult.ValueOrThrow();

        Result safetyResult = _querySafetyValidator.Validate(generatedSql);
        if (safetyResult.IsFailure)
        {
            return Result.Failure<string>(safetyResult.Error);
        }

        return Result.Success(generatedSql);
    }
}