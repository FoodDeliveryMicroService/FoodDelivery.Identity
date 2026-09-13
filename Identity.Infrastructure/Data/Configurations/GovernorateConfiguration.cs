using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data.Configurations
{
    public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Governorate> builder)
        {
            builder.ToTable("Governorates");
            
            builder.HasKey(g => g.Id);
            
            builder.Property(g => g.NameAr).HasMaxLength(100).IsRequired();
            
            builder.Property(g => g.NameEn).HasMaxLength(100).IsRequired();
            
            builder.HasIndex(x => x.NameAr).IsUnique();

            builder.HasIndex(x => x.NameEn).IsUnique();
        }
    }
}
