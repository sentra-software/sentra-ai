using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentra.Domain.Auditing;
using Sentra.Domain.DataSources;
using Sentra.Domain.Tenants;

namespace Sentra.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures persistence mapping for <see cref="AiQueryAuditLog"/>.
/// </summary>
public sealed class AiQueryAuditLogConfiguration : IEntityTypeConfiguration<AiQueryAuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiQueryAuditLog> builder)
    {
        builder.ToTable("ai_query_audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new AiQueryAuditLogId(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.DataSourceId)
            .HasConversion(id => id.Value, value => new DataSourceId(value))
            .IsRequired();

        builder.Property(x => x.IdentityUserId)
            .IsRequired();

        builder.Property(x => x.Question)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.GeneratedSql)
            .HasColumnType("text");

        builder.Property(x => x.RowCount);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ErrorCode)
            .HasMaxLength(200);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(x => x.DurationMs)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.IdentityUserId);
        builder.HasIndex(x => x.DataSourceId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}