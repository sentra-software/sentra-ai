using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="DataSource"/>.
/// </summary>
public sealed class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DataSource> builder)
    {
        builder.ToTable("data_sources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new DataSourceId(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EncryptedConnectionString)
            .HasColumnName("encrypted_connection_string")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}