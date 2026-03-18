using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Security.Identity;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="ApplicationIdentityUser"/>.
/// </summary>
public sealed class ApplicationIdentityUserConfiguration : IEntityTypeConfiguration<ApplicationIdentityUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationIdentityUser> builder)
    {
        builder.ToTable("identity_users");

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ActiveDataSourceId)
            .IsRequired(false);

        builder.HasIndex(x => new { x.TenantId, x.DomainUserId })
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.NormalizedEmail });
    }
}