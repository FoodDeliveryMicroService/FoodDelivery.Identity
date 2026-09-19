using System.Security.Claims;

namespace Identity.Application.Features.Identity.Dtos
{
    public sealed record AppUserDto(Guid UserId, string Email, IList<string> Roles, IList<Claim> Claims);
}
