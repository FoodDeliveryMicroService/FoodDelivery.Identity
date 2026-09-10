using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Identity;
using Identity.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id).IsClustered(false);

            builder.Property(rt => rt.Token).HasMaxLength(200);

            builder.HasIndex(rt => rt.Token).IsUnique();

            builder.Property(rt => rt.UserId).IsRequired();

            builder.Property(rt => rt.ExpiresOnUtc).IsRequired();
        }
    }
}
