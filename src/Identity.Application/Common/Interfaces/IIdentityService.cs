using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Application.Features.Identity.Dtos;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using Identity.Domain.Common.Results;
using Identity.Domain.Identity;

namespace Identity.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<bool> AuthorizeAsync(string userId, string? policyName);
        Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);
        Task<Result<AppUserDto>> GetUserByIdAsync(string userId);
        Task<string?> GetUserNameAsync(string userId);
        Task<Result<AppUserDto>> CreateUserAsync(string name, string email, string phoneNumber, string password, string role, CancellationToken cancellationToken = default);
        Task<Result<EmailConfirmationDto>> GenerateConfirmationCodeAsync(string userId, CancellationToken cancellationToken = default);
        Task<Result<bool>> ConfirmEmailAsync(Guid userId, string code, CancellationToken cancellationToken = default);
        Task<Result<AppUserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AppUserDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<ChangeUserRoleResponse>> ChangeUserRoleAsync(Guid userId, Role newRole, CancellationToken cancellationToken = default);
        Task<Result<AccountStatusResponse>> SuspendUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<AccountStatusResponse>> ReactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<ProfileDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<ProfileDto>> UpdateProfileAsync(Guid userId, string? name, string? phoneNumber, CancellationToken cancellationToken = default);
        Task<Result<PasswordResetTokenDto>> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<Success>> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
    }
}
