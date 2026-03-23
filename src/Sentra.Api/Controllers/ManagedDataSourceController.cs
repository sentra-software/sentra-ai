using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sentra.Api.Models;
using Sentra.Api.Models.ManagedDataSources;
using Sentra.Application.Abstractions.DataSources;
using Sentra.Application.Abstractions.Licensing;
using Sentra.Application.Abstractions.Security;
using Sentra.Application.DataSources;
using Sentra.Connectors.Abstractions.Connectors;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.Infrastructure.Persistence;
using Sentra.Security.Extensions;
using Sentra.Security.Identity;
using Sentra.SharedKernel.Results;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides secure tenant-owned managed data source endpoints.
/// </summary>
[ApiController]
[Authorize(Policy = "CanManageDataSources")]
[EnableRateLimiting("managed-data-sources")]
[Route("api/managed-data-sources")]
public sealed class ManagedDataSourcesController : ControllerBase
{
    private readonly SentraPlatformDbContext _dbContext;
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly IDataSourceConnectionService _connectionService;
    private readonly IConnectionStringProtector _connectionStringProtector;
    private readonly ICurrentTenantLicenseService _licenseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedDataSourcesController"/> class.
    /// </summary>
    public ManagedDataSourcesController(
        SentraPlatformDbContext dbContext,
        UserManager<ApplicationIdentityUser> userManager,
        IDataSourceConnectionService connectionService,
        IConnectionStringProtector connectionStringProtector,
        ICurrentTenantLicenseService licenseService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _connectionService = connectionService;
        _connectionStringProtector = connectionStringProtector;
        _licenseService = licenseService;
    }

    /// <summary>
    /// Gets all managed data sources for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ManagedDataSourceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Guid tenantIdValue = User.GetTenantId();
        if (tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        TenantId tenantId = new(tenantIdValue);

        List<ManagedDataSourceResponse> items = await _dbContext.DataSources
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .Select(x => new ManagedDataSourceResponse
            {
                Id = ((DataSourceId)x.Id).Value,
                Name = x.Name,
                DataSourceType = x.Type.ToString(),
                Status = x.Status.ToString(),
                CreatedAtUtc = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    /// <summary>
    /// Gets the active managed data source for the current user.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ActiveManagedDataSourceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        Guid identityUserId = User.GetIdentityUserId();
        Guid tenantIdValue = User.GetTenantId();

        if (identityUserId == Guid.Empty || tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        ApplicationIdentityUser? identityUser = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == identityUserId, cancellationToken);

        if (identityUser is null || identityUser.ActiveDataSourceId is null)
        {
            return Ok(new ActiveManagedDataSourceResponse());
        }

        TenantId tenantId = new(tenantIdValue);
        DataSourceId dataSourceId = new(identityUser.ActiveDataSourceId.Value);

        DataSource? dataSource = await _dbContext.DataSources
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == dataSourceId && x.TenantId == tenantId,
                cancellationToken);

        if (dataSource is null)
        {
            return Ok(new ActiveManagedDataSourceResponse());
        }

        return Ok(new ActiveManagedDataSourceResponse
        {
            DataSourceId = ((DataSourceId)dataSource.Id).Value,
            Name = dataSource.Name,
            DataSourceType = dataSource.Type.ToString(),
            Status = dataSource.Status.ToString()
        });
    }

    /// <summary>
    /// Creates a new managed data source for the current tenant.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ManagedDataSourceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
    [FromBody] CreateManagedDataSourceRequest request,
    CancellationToken cancellationToken)
    {
        Guid tenantIdValue = User.GetTenantId();
        if (tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        Result licenseResult = await _licenseService.EnsureManagedDataSourceCreationAllowedAsync(
            tenantIdValue,
            cancellationToken);

        if (licenseResult.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                licenseResult.Error.Code,
                licenseResult.Error.Message));
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.ConnectionString))
        {
            return BadRequest(new ApiErrorResponse(
                "managed_data_sources.invalid_request",
                "Name, data source type and connection string are required."));
        }

        DataSourceType dataSourceType = request.DataSourceType;

        Result<ConnectionTestResult> connectionTest = await _connectionService.TestConnectionAsync(
            dataSourceType,
            request.ConnectionString,
            cancellationToken);

        if (connectionTest.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                connectionTest.Error.Code,
                connectionTest.Error.Message));
        }

        string encryptedConnectionString = _connectionStringProtector.Protect(request.ConnectionString);

        Result createResult = DataSource.Create(
            new TenantId(tenantIdValue),
            request.Name,
            dataSourceType,
            encryptedConnectionString);

        if (createResult.IsFailure)
        {
            return BadRequest(new ApiErrorResponse(
                createResult.Error.Code,
                createResult.Error.Message));
        }

        DataSource dataSource = ((Result<DataSource>)createResult).ValueOrThrow();
        dataSource.Activate();

        _dbContext.DataSources.Add(dataSource);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = ((DataSourceId)dataSource.Id).Value },
            new ManagedDataSourceResponse
            {
                Id = ((DataSourceId)dataSource.Id).Value,
                Name = dataSource.Name,
                DataSourceType = dataSource.Type.ToString(),
                Status = dataSource.Status.ToString(),
                CreatedAtUtc = dataSource.CreatedAt
            });
    }

    /// <summary>
    /// Sets the active managed data source for the current user.
    /// </summary>
    [HttpPost("set-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetActive(
        [FromBody] SetActiveManagedDataSourceRequest request,
        CancellationToken cancellationToken)
    {
        Guid identityUserId = User.GetIdentityUserId();
        Guid tenantIdValue = User.GetTenantId();

        if (identityUserId == Guid.Empty || tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        ApplicationIdentityUser? identityUser = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Id == identityUserId, cancellationToken);

        if (identityUser is null)
        {
            return Unauthorized();
        }

        TenantId tenantId = new(tenantIdValue);
        DataSourceId dataSourceId = new(request.DataSourceId);

        bool exists = await _dbContext.DataSources
            .AnyAsync(
                x => x.Id == dataSourceId &&
                     x.TenantId == tenantId &&
                     x.Status == DataSourceStatus.Active,
                cancellationToken);

        if (!exists)
        {
            return BadRequest(new ApiErrorResponse(
                "managed_data_sources.not_found",
                "The selected active data source does not exist for this tenant or is not active."));
        }

        identityUser.ActiveDataSourceId = request.DataSourceId;
        IdentityResult updateResult = await _userManager.UpdateAsync(identityUser);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new ApiErrorResponse(
                "managed_data_sources.active_update_failed",
                string.Join("; ", updateResult.Errors.Select(x => x.Description))));
        }

        return NoContent();
    }

    /// <summary>
    /// Validates and refreshes the status of a managed data source.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(ManagedDataSourceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(
        Guid id,
        CancellationToken cancellationToken)
    {
        Guid tenantIdValue = User.GetTenantId();
        if (tenantIdValue == Guid.Empty)
        {
            return Unauthorized();
        }

        TenantId tenantId = new(tenantIdValue);
        DataSourceId dataSourceId = new(id);

        DataSource? dataSource = await _dbContext.DataSources
            .FirstOrDefaultAsync(
                x => x.Id == dataSourceId && x.TenantId == tenantId,
                cancellationToken);

        if (dataSource is null)
        {
            return BadRequest(new ApiErrorResponse(
                "managed_data_sources.not_found",
                "The managed data source could not be found."));
        }

        string connectionString = _connectionStringProtector.Unprotect(dataSource.EncryptedConnectionString);

        Result<ConnectionTestResult> testResult = await _connectionService.TestConnectionAsync(
            dataSource.Type,
            connectionString,
            cancellationToken);

        if (testResult.IsFailure)
        {
            dataSource.Disable();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return BadRequest(new ApiErrorResponse(
                testResult.Error.Code,
                testResult.Error.Message));
        }

        dataSource.Activate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new ManagedDataSourceResponse
        {
            Id = ((DataSourceId)dataSource.Id).Value,
            Name = dataSource.Name,
            DataSourceType = dataSource.Type.ToString(),
            Status = dataSource.Status.ToString(),
            CreatedAtUtc = dataSource.CreatedAt
        });
    }
}