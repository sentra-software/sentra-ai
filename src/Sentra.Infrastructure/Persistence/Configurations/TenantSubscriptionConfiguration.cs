using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Domain.Licensing;
using Sentra.Domain.Tenants;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="TenantSubscription"/>.
/// </summary>
public sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("tenant_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new TenantSubscriptionId(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId)
            .IsUnique();

        builder.Property(x => x.LicensePlanId)
            .HasConversion(id => id.Value, value => new LicensePlanId(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.StartsAtUtc)
            .IsRequired();

        builder.Property(x => x.EndsAtUtc);

        builder.Property(x => x.TrialEndsAtUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}