using Sentra.Application.Abstractions.Auditing;
using Sentra.Domain.Auditing;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.SharedKernel.Results;

namespace Sentra.Infrastructure.Persistence;

/// <summary>
/// Persists AI query audit logs using the platform database context.
/// </summary>
public sealed class AiQueryAuditLogWriter : IAiQueryAuditLogWriter
{
    private readonly SentraPlatformDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiQueryAuditLogWriter"/> class.
    /// </summary>
    /// <param name="dbContext">The platform database context.</param>
    public AiQueryAuditLogWriter(SentraPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task WriteAsync(
        AiQueryAuditLogEntry entry,
        CancellationToken cancellationToken = default
    )
    {
        Result createResult = AiQueryAuditLog.Create(
            new TenantId(entry.TenantId),
            entry.IdentityUserId,
            new DataSourceId(entry.DataSourceId),
            entry.Question,
            entry.GeneratedSql,
            entry.RowCount,
            entry.Status,
            entry.ErrorCode,
            entry.ErrorMessage,
            entry.DurationMs
        );

        if (createResult.IsFailure)
        {
            return;
        }

        AiQueryAuditLog log = ((Result<AiQueryAuditLog>)createResult).ValueOrThrow();

        _dbContext.AiQueryAuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}