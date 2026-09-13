using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;

namespace Identity.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string role);

        Task<bool> AuthorizeAsync(string userId, string? policyName);

        Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);

        Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

        Task<string?> GetUserNameAsync(string userId);
        Task<Result<AppUserDto>> CreateUserAsync(
            string name,
            string email,
            string phoneNumber,
            string password,
            string role,
            CancellationToken cancellationToken = default);

        // NEW: Generate and save a confirmation code
        Task<Result<EmailConfirmationDto>> GenerateConfirmationCodeAsync(
            string userId,
            CancellationToken cancellationToken = default);

        // NEW: Validate and confirm the email
        Task<Result<bool>> ConfirmEmailAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default);
        Task<Result<AppUserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AppUserDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    }
}
