using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sentra.Api.Models.Auth;
using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;
using Sentra.Domain.Users;
using Sentra.Infrastructure.Persistence;
using Sentra.Security.Identity;
using Sentra.Security.Jwt;
using Sentra.SharedKernel.Results;
using DomainUser = Sentra.Domain.Users.User;

namespace Sentra.Api.Controllers;

/// <summary>
/// Provides authentication endpoints for the Sentra platform.
/// </summary>
[ApiController]
[EnableRateLimiting("auth")]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly SentraPlatformDbContext _dbContext;
    private readonly UserManager<ApplicationIdentityUser> _userManager;
    private readonly RoleManager<ApplicationIdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="dbContext">The platform database context.</param>
    /// <param name="userManager">The identity user manager.</param>
    /// <param name="roleManager">The identity role manager.</param>
    /// <param name="jwtTokenGenerator">The JWT token generator.</param>
    public AuthController(
        SentraPlatformDbContext dbContext,
        UserManager<ApplicationIdentityUser> userManager,
        RoleManager<ApplicationIdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// Registers a new Sentra account, creates a tenant,
    /// and provisions the Starter plan as a trial subscription.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A JWT access token when registration succeeds.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Tenant name, email, display name and password are required.");
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        ApplicationIdentityUser? existingIdentityUser =
            await _userManager.FindByEmailAsync(normalizedEmail);

        if (existingIdentityUser is not null)
        {
            return BadRequest("An account with this email address already exists.");
        }

        Result<Tenant> tenantResult = Tenant.Create(request.TenantName);
        if (tenantResult.IsFailure)
        {
            return BadRequest(tenantResult.Error.Message);
        }

        Tenant tenant = tenantResult.Value!;

        Result<DomainUser> domainUserResult = DomainUser.Create(
            (TenantId)tenant.Id,
            request.Email,
            request.DisplayName,
            UserRole.Owner);

        if (domainUserResult.IsFailure)
        {
            return BadRequest(domainUserResult.Error.Message);
        }

        DomainUser domainUser = domainUserResult.Value!;

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Tenants.Add(tenant);
        _dbContext.PlatformUsers.Add(domainUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        const string ownerRole = "Owner";

        if (!await _roleManager.RoleExistsAsync(ownerRole))
        {
            IdentityResult createRoleResult =
                await _roleManager.CreateAsync(new ApplicationIdentityRole(ownerRole));

            if (!createRoleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(createRoleResult.Errors.Select(x => x.Description));
            }
        }

        ApplicationIdentityUser identityUser = new()
        {
            Id = Guid.NewGuid(),
            DomainUserId = ((UserId)domainUser.Id).Value,
            TenantId = ((TenantId)tenant.Id).Value,
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = domainUser.DisplayName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            EmailConfirmed = true
        };

        IdentityResult createUserResult =
            await _userManager.CreateAsync(identityUser, request.Password);

        if (!createUserResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(createUserResult.Errors.Select(x => x.Description));
        }

        IdentityResult addRoleResult =
            await _userManager.AddToRoleAsync(identityUser, ownerRole);

        if (!addRoleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(addRoleResult.Errors.Select(x => x.Description));
        }

        LicensePlan? starterPlan = await _dbContext.LicensePlans
            .FirstOrDefaultAsync(x => x.Code == "STARTER" && x.IsActive, cancellationToken);

        if (starterPlan is null)
        {
            Result starterPlanResult = LicensePlan.Create(
                name: "Starter",
                code: "STARTER",
                maxManagedDataSources: 1,
                maxQueriesPerMonth: 250,
                previewEnabled: true,
                auditRetentionDays: 30,
                mySqlEnabled: false,
                sqliteEnabled: false,
                pricePerMonth: 0m);

            if (starterPlanResult.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(starterPlanResult.Error.Message);
            }

            starterPlan = ((Result<LicensePlan>)starterPlanResult).ValueOrThrow();

            _dbContext.LicensePlans.Add(starterPlan);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        bool subscriptionAlreadyExists = await _dbContext.TenantSubscriptions
            .AnyAsync(x => x.TenantId == (TenantId)tenant.Id, cancellationToken);

        if (subscriptionAlreadyExists)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest("A subscription already exists for this tenant.");
        }

        Result subscriptionResult = TenantSubscription.CreateTrial(
            (TenantId)tenant.Id,
            starterPlan.Id,
            trialDays: 14);

        if (subscriptionResult.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(subscriptionResult.Error.Message);
        }

        TenantSubscription subscription =
            ((Result<TenantSubscription>)subscriptionResult).ValueOrThrow();

        _dbContext.TenantSubscriptions.Add(subscription);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        string accessToken = _jwtTokenGenerator.GenerateToken(
            identityUser,
            [ownerRole],
            domainUser.Role.ToString());

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            UserId = identityUser.Id,
            TenantId = identityUser.TenantId,
            DisplayName = identityUser.DisplayName,
            Email = identityUser.Email ?? string.Empty
        });
    }

    /// <summary>
    /// Authenticates an existing account.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>A JWT access token when authentication succeeds.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized("Invalid credentials.");
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        ApplicationIdentityUser? identityUser =
            await _userManager.FindByEmailAsync(normalizedEmail);

        if (identityUser is null || !identityUser.IsActive)
        {
            return Unauthorized("Invalid credentials.");
        }

        if (await _userManager.IsLockedOutAsync(identityUser))
        {
            return Unauthorized("This account is temporarily locked.");
        }

        bool passwordValid =
            await _userManager.CheckPasswordAsync(identityUser, request.Password);

        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(identityUser);
            return Unauthorized("Invalid credentials.");
        }

        await _userManager.ResetAccessFailedCountAsync(identityUser);

        IList<string> roles = await _userManager.GetRolesAsync(identityUser);

        UserId domainUserId = new(identityUser.DomainUserId);
        DomainUser? platformUser = await _dbContext.PlatformUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == domainUserId);
        
        if(platformUser is null)
        {
            return Unauthorized("The linked platform user could not be found.");
        }

        string accessToken = _jwtTokenGenerator.GenerateToken(identityUser, roles.ToArray(), platformUser!.Role.ToString());

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            UserId = identityUser.Id,
            TenantId = identityUser.TenantId,
            DisplayName = identityUser.DisplayName,
            Email = identityUser.Email ?? string.Empty
        });
    }
}