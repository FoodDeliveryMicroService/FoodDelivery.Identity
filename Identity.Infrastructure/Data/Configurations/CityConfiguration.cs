using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Data.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NameAr)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.NameEn)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne<Governorate>()
                .WithMany()
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.GovernorateId, x.NameAr })
                .IsUnique();

            builder.HasIndex(x => new { x.GovernorateId, x.NameEn })
                .IsUnique();
        }
    }
}
