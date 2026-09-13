using Identity.Domain.Email;
using Identity.Domain.Identity.Entities;
using Identity.Domain.Location;
using Microsoft.EntityFrameworkCore;


namespace Identity.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; }
        public DbSet<Address> Addresses { get; }
        public DbSet<City> Cities {  get; }
        public DbSet<Governorate> Governorates {  get; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
