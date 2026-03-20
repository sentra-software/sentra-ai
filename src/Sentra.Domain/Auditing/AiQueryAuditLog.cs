using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Abstractions;
using Sentra.SharedKernel.Results;

namespace Sentra.Domain.Auditing;

/// <summary>
/// Represents a single audited AI query interaction.
/// </summary>
public sealed class AiQueryAuditLog : AggregateRoot<AiQueryAuditLogId>
{
    private AiQueryAuditLog(
        AiQueryAuditLogId id,
        TenantId tenantId,
        Guid identityUserId,
        DataSourceId dataSourceId,
        string question,
        string? generatedSql,
        int? rowCount,
        AiQueryAuditStatus status,
        string? errorCode,
        string? errorMessage,
        int durationMs)
        : base(id)
    {
        TenantId = tenantId;
        IdentityUserId = identityUserId;
        DataSourceId = dataSourceId;
        Question = question;
        GeneratedSql = generatedSql;
        RowCount = rowCount;
        Status = status;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        DurationMs = durationMs;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// Gets the authenticated identity user identifier.
    /// </summary>
    public Guid IdentityUserId { get; }

    /// <summary>
    /// Gets the managed data source identifier.
    /// </summary>
    public DataSourceId DataSourceId { get; }

    /// <summary>
    /// Gets the original natural-language question.
    /// </summary>
    public string Question { get; }

    /// <summary>
    /// Gets the generated SQL, if available.
    /// </summary>
    public string? GeneratedSql { get; }

    /// <summary>
    /// Gets the returned row count, if available.
    /// </summary>
    public int? RowCount { get; }

    /// <summary>
    /// Gets the audit status.
    /// </summary>
    public AiQueryAuditStatus Status { get; }

    /// <summary>
    /// Gets the error code, if available.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the error message, if available.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets the execution duration in milliseconds.
    /// </summary>
    public int DurationMs { get; }

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Creates a new AI query audit log entry.
    /// </summary>
    public static Result Create(
        TenantId tenantId,
        Guid identityUserId,
        DataSourceId dataSourceId,
        string question,
        string? generatedSql,
        int? rowCount,
        AiQueryAuditStatus status,
        string? errorCode,
        string? errorMessage,
        int durationMs)
    {
        if (tenantId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "audit.tenant.required",
                "Tenant identifier is required."));
        }

        if (identityUserId == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "audit.user.required",
                "Identity user identifier is required."));
        }

        if (dataSourceId.Value == Guid.Empty)
        {
            return Result.Failure(Error.Validation(
                "audit.datasource.required",
                "Data source identifier is required."));
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return Result.Failure(Error.Validation(
                "audit.question.required",
                "Question is required."));
        }

        if (durationMs < 0)
        {
            return Result.Failure(Error.Validation(
                "audit.duration.invalid",
                "Duration must be zero or greater."));
        }

        AiQueryAuditLog auditLog = new AiQueryAuditLog(
            AiQueryAuditLogId.New(),
            tenantId,
            identityUserId,
            dataSourceId,
            question.Trim(),
            string.IsNullOrWhiteSpace(generatedSql) ? null : generatedSql.Trim(),
            rowCount,
            status,
            string.IsNullOrWhiteSpace(errorCode) ? null : errorCode.Trim(),
            string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            durationMs);

        return Result.Success(auditLog);
    }
}