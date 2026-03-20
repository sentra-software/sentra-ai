using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sentra.Domain.Auditing;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;
using Sentra.Domain.Users;
using Sentra.Security.Identity;

namespace Sentra.Infrastructure.Persistence;

/// <summary>
/// Represents the primary platform database context for Sentra.
/// </summary>
public sealed class SentraPlatformDbContext
    : IdentityDbContext<ApplicationIdentityUser, ApplicationIdentityRole, Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SentraPlatformDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public SentraPlatformDbContext(DbContextOptions<SentraPlatformDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the tenant set.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets the domain user set.
    /// </summary>
    public DbSet<User> PlatformUsers => Set<User>();

    /// <summary>
    /// Gets the data source set.
    /// </summary>
    public DbSet<DataSource> DataSources => Set<DataSource>();

    /// <summary>
    /// Gets the AI query audit log set.
    /// </summary>
    public DbSet<AiQueryAuditLog> AiQueryAuditLogs => Set<AiQueryAuditLog>();

    /// <summary>
    /// Configures the EF Core model.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(SentraPlatformDbContext).Assembly);
    }
}