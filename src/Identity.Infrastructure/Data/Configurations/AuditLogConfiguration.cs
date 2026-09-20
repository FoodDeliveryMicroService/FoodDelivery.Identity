using Identity.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TargetUserId)
            .IsRequired();

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.OldValue)
            .HasMaxLength(500);

        builder.Property(a => a.NewValue)
            .HasMaxLength(500);

        builder.HasIndex(a => a.TargetUserId);
        builder.HasIndex(a => a.CreatedAtUtc);
    }
}