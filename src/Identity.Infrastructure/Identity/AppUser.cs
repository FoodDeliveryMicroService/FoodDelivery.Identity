using Identity.Domain.Identity.Enums;
using Identity.Domain.Location;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public ICollection<Address> Addresses { get; private set; } = [];
    }

}
