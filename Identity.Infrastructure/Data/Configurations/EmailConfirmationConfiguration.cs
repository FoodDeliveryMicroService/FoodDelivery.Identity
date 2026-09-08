using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Email;

namespace Identity.Infrastructure.Data.Configurations;

public class EmailConfirmationConfiguration : IEntityTypeConfiguration<EmailConfirmation>
{
    public void Configure(EntityTypeBuilder<EmailConfirmation> builder)
    {
        builder.ToTable("EmailConfirmations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(36); // Assuming GUID string length

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(e => e.ExpiresAt)
            .IsRequired();

        builder.Property(e => e.IsUsed)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.AttemptCount)
            .IsRequired();

        // Index for fast lookups
        builder.HasIndex(e => new { e.UserId, e.IsUsed });
    }
}