using Identity.Domain.Identity;

namespace Identity.Application.Features.Authentication.Dtos.RegisterUser
{
    public sealed record RegisterUserRequest(
        string Name, string Email, string PhoneNumber, string Password,Role Role);
}
