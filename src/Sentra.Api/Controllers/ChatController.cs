using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sentra.Api.Models;
using Sentra.Api.Models.Chat;
using Sentra.Application.Abstractions.AI;
using Sentra.Application.Abstractions.Auditing;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Application.Abstractions.Security;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Persistence;
using Sentra.Security.Extensions;
using Sentra.Security.Identity;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides chat-based endpoints for Sentra AI operations.
/// </summary>
[ApiController]
[Authorize(Policy = "CanUseChat")]
[EnableRateLimiting("ai-chat")]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    /// <inheritdoc/>
    [HttpPost("ask")]
    [ProducesResponseType(typeof(AskQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask(
        [FromBody] AskQuestionRequest request,
        [FromServices] IAskQuestionService service,
        [FromServices] SentraPlatformDbContext dbContext,
        [FromServices] UserManager<ApplicationIdentityUser> userManager,
        [FromServices] IConnectionStringProtector connectionStringProtector,
        [FromServices] ICurrentTenantLicenseService licenseService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new ApiErrorResponse(
                "chat.question.required",
                "A question is required."));
        }

        Guid identityUserId = User.GetIdentityUserId();
        Guid tenantIdValue = User.GetTenantId();

        if (identityUserId == Guid.Empty || tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        ApplicationIdentityUser? identityUser = await userManager.Users
            .FirstOrDefaultAsync(x => x.Id == identityUserId, cancellationToken);

        if (identityUser is null)
        {
            return Unauthorized();
        }

        Result queryLicenseResult = await licenseService.EnsureQueryAllowedAsync(
            tenantIdValue, cancellationToken
        );

        if(queryLicenseResult.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                queryLicenseResult.Error.Code,
                queryLicenseResult.Error.Message));
        }

        Guid? selectedDataSourceId = request.DataSourceId ?? identityUser.ActiveDataSourceId;
        if (selectedDataSourceId is null || selectedDataSourceId == Guid.Empty)
        {
            return BadRequest(new ApiErrorResponse(
                "chat.datasource.required",
                "No managed data source was specified and no active data source is selected."));
        }

        TenantId tenantId = new(tenantIdValue);
        DataSourceId dataSourceId = new(selectedDataSourceId.Value);

        DataSource? dataSource = await dbContext.DataSources
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == dataSourceId &&
                     x.TenantId == tenantId &&
                     x.Status == DataSourceStatus.Active,
                cancellationToken);

        if (dataSource is null)
        {
            return BadRequest(new ApiErrorResponse(
                "chat.datasource.invalid",
                "The selected managed data source is not available for this tenant."));
        }

        string connectionString =
            connectionStringProtector.Unprotect(dataSource.EncryptedConnectionString);

        Result<AskQuestionResult> result = await service.AskAsync(
            dataSource.Type,
            connectionString,
            request.Question,
            new AskQuestionAuditContext(
                tenantIdValue,
                identityUserId,
                selectedDataSourceId.Value),
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        AskQuestionResult value = result.ValueOrThrow();

        return Ok(new AskQuestionResponse(
            value.Question,
            value.GeneratedSql,
            value.QueryResult.RowCount,
            value.Answer));
    }

    [HttpPost("preview")]
    [ProducesResponseType(typeof(PreviewQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Preview(
    [FromBody] AskQuestionRequest request,
    [FromServices] IPreviewQuestionService service,
    [FromServices] SentraPlatformDbContext dbContext,
    [FromServices] UserManager<ApplicationIdentityUser> userManager,
    [FromServices] IConnectionStringProtector connectionStringProtector,
    [FromServices] ICurrentTenantLicenseService licenseService,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new ApiErrorResponse(
                "chat.question.required",
                "A question is required."));
        }

        Guid identityUserId = User.GetIdentityUserId();
        Guid tenantIdValue = User.GetTenantId();

        if (identityUserId == Guid.Empty || tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        ApplicationIdentityUser? identityUser = await userManager.Users
            .FirstOrDefaultAsync(x => x.Id == identityUserId, cancellationToken);

        if (identityUser is null)
        {
            return Unauthorized();
        }

        Result previewLicenseResult = await licenseService.EnsurePreviewAllowedAsync(
            tenantIdValue, cancellationToken
        );

        if(previewLicenseResult.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                previewLicenseResult.Error.Code,
                previewLicenseResult.Error.Message));
        }

        Guid? selectedDataSourceId = request.DataSourceId ?? identityUser.ActiveDataSourceId;
        if (selectedDataSourceId is null || selectedDataSourceId == Guid.Empty)
        {
            return BadRequest(new ApiErrorResponse(
                "chat.datasource.required",
                "No managed data source was specified and no active data source is selected."));
        }

        TenantId tenantId = new(tenantIdValue);
        DataSourceId dataSourceId = new(selectedDataSourceId.Value);

        DataSource? dataSource = await dbContext.DataSources
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == dataSourceId &&
                     x.TenantId == tenantId &&
                     x.Status == DataSourceStatus.Active,
                cancellationToken);

        if (dataSource is null)
        {
            return BadRequest(new ApiErrorResponse(
                "chat.datasource.invalid",
                "The selected managed data source is not available for this tenant."));
        }

        string connectionString =
            connectionStringProtector.Unprotect(dataSource.EncryptedConnectionString);

        Result<string> result = await service.PreviewAsync(
            dataSource.Type,
            connectionString,
            request.Question,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                result.Error.Code,
                result.Error.Message));
        }

        return Ok(new PreviewQuestionResponse
        {
            Question = request.Question,
            GeneratedSql = result.ValueOrThrow(),
            IsSafe = true
        });
    }
}