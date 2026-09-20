using Identity.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
