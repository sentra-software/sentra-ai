using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Security.Identity;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="ApplicationIdentityRole"/>.
/// </summary>
public sealed class ApplicationIdentityRoleConfiguration : IEntityTypeConfiguration<ApplicationIdentityRole>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationIdentityRole> builder)
    {
        builder.ToTable("identity_roles");
    }
}