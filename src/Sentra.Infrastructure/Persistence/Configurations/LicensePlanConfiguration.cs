using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Domain.Licensing;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="LicensePlan"/>.
/// </summary>
public sealed class LicensePlanConfiguration : IEntityTypeConfiguration<LicensePlan>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LicensePlan> builder)
    {
        builder.ToTable("license_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new LicensePlanId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.MaxManagedDataSources)
            .IsRequired();

        builder.Property(x => x.MaxQueriesPerMonth)
            .IsRequired();

        builder.Property(x => x.PreviewEnabled)
            .IsRequired();

        builder.Property(x => x.AuditRetentionDays)
            .IsRequired();

        builder.Property(x => x.MySqlEnabled)
            .IsRequired();

        builder.Property(x => x.SqliteEnabled)
            .IsRequired();

        builder.Property(x => x.PricePerMonth)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}