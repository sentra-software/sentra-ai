using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sentra.Api.Models.Auditing;
using Sentra.Domain.Auditing;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Persistence;
using Sentra.Security.Extensions;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides read access to AI query audit logs for the current tenant.
/// </summary>
[ApiController]
[Authorize(Policy = "CanViewAuditLogs")]
[EnableRateLimiting("audit-api")]
[Route("api/audit-logs")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly SentraPlatformDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogsController"/> class.
    /// </summary>
    /// <param name="dbContext">The platform database context.</param>
    public AuditLogsController(SentraPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets paged audit logs for the current tenant.
    /// </summary>
    /// <param name="dataSourceId">Optional data source filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="fromUtc">Optional lower UTC bound.</param>
    /// <param name="toUtc">Optional upper UTC bound.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged audit log result.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedAiQueryAuditLogResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? dataSourceId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        Guid tenantIdValue = User.GetTenantId();
        if (tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<AiQueryAuditLog> query = _dbContext.AiQueryAuditLogs
            .AsNoTracking()
            .Where(x => x.TenantId == new TenantId(tenantIdValue));

        if (dataSourceId.HasValue && dataSourceId.Value != Guid.Empty)
        {
            DataSourceId typedDataSourceId = new(dataSourceId.Value);
            query = query.Where(x => x.DataSourceId == typedDataSourceId);
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<AiQueryAuditStatus>(status, true, out AiQueryAuditStatus parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= toUtc.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<AiQueryAuditLogResponse> items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AiQueryAuditLogResponse
            {
                Id = ((AiQueryAuditLogId)x.Id).Value,
                IdentityUserId = x.IdentityUserId,
                DataSourceId = x.DataSourceId.Value,
                Question = x.Question,
                GeneratedSql = x.GeneratedSql,
                RowCount = x.RowCount,
                Status = x.Status.ToString(),
                ErrorCode = x.ErrorCode,
                ErrorMessage = x.ErrorMessage,
                DurationMs = x.DurationMs,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(new PagedAiQueryAuditLogResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        });
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(AiQueryAuditStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(
    [FromQuery] Guid? dataSourceId,
    [FromQuery] DateTime? fromUtc,
    [FromQuery] DateTime? toUtc,
    CancellationToken cancellationToken = default)
    {
        Guid tenantIdValue = User.GetTenantId();
        if (tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        IQueryable<AiQueryAuditLog> query = _dbContext.AiQueryAuditLogs
            .AsNoTracking()
            .Where(x => x.TenantId == new TenantId(tenantIdValue));

        if (dataSourceId.HasValue && dataSourceId.Value != Guid.Empty)
        {
            DataSourceId typedDataSourceId = new(dataSourceId.Value);
            query = query.Where(x => x.DataSourceId == typedDataSourceId);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= toUtc.Value);
        }

        int totalQueries = await query.CountAsync(cancellationToken);
        int succeededCount = await query.CountAsync(x => x.Status == AiQueryAuditStatus.Succeeded, cancellationToken);
        int failedCount = await query.CountAsync(x => x.Status == AiQueryAuditStatus.Failed, cancellationToken);
        int blockedCount = await query.CountAsync(x => x.Status == AiQueryAuditStatus.Blocked, cancellationToken);

        decimal avgDurationMs = totalQueries == 0
            ? 0
            : await query.AverageAsync(x => (decimal)x.DurationMs, cancellationToken);

        DateTime last24Hours = DateTime.UtcNow.AddHours(-24);
        int queriesLast24Hours = await query.CountAsync(x => x.CreatedAtUtc >= last24Hours, cancellationToken);

        decimal successRate = totalQueries == 0
            ? 0
            : Math.Round((decimal)succeededCount / totalQueries * 100m, 2);

        return Ok(new AiQueryAuditStatsResponse
        {
            TotalQueries = totalQueries,
            SucceededCount = succeededCount,
            FailedCount = failedCount,
            BlockedCount = blockedCount,
            SuccessRate = successRate,
            AvgDurationMs = Math.Round(avgDurationMs, 2),
            QueriesLast24Hours = queriesLast24Hours
        });
    }
}