using System.Security.Cryptography;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Application.Features.Identity.Dtos;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using Identity.Domain.Common.Results;
using Identity.Domain.Email;
using Identity.Domain.Identity;
using Identity.Domain.Identity.Enums;
using Identity.Domain.Identity.Errors;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services
{
    public class IdentityService(
        UserManager<AppUser> userManager,
        IAppDbContext context, // was: AppDbContext _context — now depends on the abstraction (ARC-02)
        IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService) : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IAppDbContext _context = context;
        private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        private readonly IAuthorizationService _authorizationService = authorizationService;

        public async Task<bool> IsInRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<bool> AuthorizeAsync(string userId, string? policyName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
            var result = await _authorizationService.AuthorizeAsync(principal, policyName!);
            return result.Succeeded;
        }

        public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Error.NotFound("User_Not_Found", $"User with email {UtilityService.MaskEmail(email)} not found");

            if (!user.EmailConfirmed)
                return Error.Conflict("Email_Not_Confirmed", $"email '{UtilityService.MaskEmail(email)}' not confirmed");

            if (!await _userManager.CheckPasswordAsync(user, password))
                return Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");

            return new AppUserDto(user.Id, user.Email!, await _userManager.GetRolesAsync(user), await _userManager.GetClaimsAsync(user));
        }

        public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(nameof(userId));
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            return new AppUserDto(user.Id, user.Email!, roles, claims);
        }

        public async Task<string?> GetUserNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.Name;
        }

        public async Task<Result<AppUserDto>> CreateUserAsync(
            string name,
            string email,
            string phoneNumber,
            string password,
            string role,
            CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
                return Error.Conflict("DuplicateEmail", $"Email '{UtilityService.MaskEmail(email)}' is already registered.");

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                Name = name,
                PhoneNumber = phoneNumber
            };

            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .Select(e => Error.Validation("IdentityError", e.Description))
                    .ToList();
                return errors;
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return Error.Validation("RoleAssignmentFailed", $"Failed to assign role '{role}'.");
            }

            return new AppUserDto(
                user.Id,
                user.Email!,
                await _userManager.GetRolesAsync(user),
                await _userManager.GetClaimsAsync(user)
            );
        }

        public async Task<Result<EmailConfirmationDto>> GenerateConfirmationCodeAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Error.NotFound("UserNotFound", "User not found.");

            if (user.EmailConfirmed)
                return EmailConfirmationErrors.AlreadyConfirmed;

            // Rate limiting: reject if 3+ codes were already generated in the last 15 minutes
            var windowStart = DateTimeOffset.UtcNow.AddMinutes(-15);
            var recentCount = await _context.EmailConfirmations
                .CountAsync(c => c.UserId == userId && c.CreatedAt >= windowStart, cancellationToken);

            if (recentCount >= 3)
                return EmailConfirmationErrors.TooManyRequests; // distinct from TooManyAttempts (wrong-code case)

            // Generate a cryptographically secure 6-digit code (not System.Random)
            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            // The entity hashes the code internally — CodeHash is what actually gets persisted
            var confirmation = new EmailConfirmation(
                userId,
                code,
                DateTimeOffset.UtcNow.AddMinutes(15) // code expires 15 minutes from now
            );

            _context.EmailConfirmations.Add(confirmation);
            await _context.SaveChangesAsync(cancellationToken);

            // Return the plaintext `code` local variable (never confirmation.CodeHash) — this is
            // the only place the raw code exists after this point, and it's only used for the email
            return new EmailConfirmationDto(
                confirmation.Id,
                code,
                confirmation.ExpiresAt
            );
        }

        public async Task<Result<bool>> ConfirmEmailAsync(
            Guid userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return Error.NotFound("UserNotFound", "User not found.");

            if (user.EmailConfirmed)
                return EmailConfirmationErrors.AlreadyConfirmed;

            var confirmation = await _context.EmailConfirmations
                .Where(c => c.UserId == userId.ToString() && !c.IsUsed)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (confirmation is null)
                return EmailConfirmationErrors.InvalidCode;

            // Compare using the entity's hash-and-fixed-time-equals method — never raw string equality
            if (!confirmation.VerifyCode(code))
            {
                confirmation.IncrementAttempt();
                await _context.SaveChangesAsync(cancellationToken);
                return EmailConfirmationErrors.InvalidCode;
            }

            var useResult = confirmation.Use();
            if (useResult.IsError)
                return useResult.Errors;

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<Result<AppUserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Error.NotFound("UserNotFound", $"User with email '{UtilityService.MaskEmail(email)}' not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            return new AppUserDto(user.Id, user.Email!, roles, claims);
        }

        public async Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user?.EmailConfirmed ?? false;
        }

        public async Task<Result<AppUserDto>> LoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return AuthenticationErrors.InvalidCredentials;

            if (user.Status == AccountStatus.Suspended)
                return AuthenticationErrors.AccountSuspended;

            if (!user.EmailConfirmed)
                return AuthenticationErrors.AccountNotConfirmed;


            if (!await _userManager.CheckPasswordAsync(user, password))
                return AuthenticationErrors.InvalidCredentials;

            return new AppUserDto(
                user.Id,
                user.Email!,
                await _userManager.GetRolesAsync(user),
                await _userManager.GetClaimsAsync(user)
            );
        }

        public async Task<Result<ChangeUserRoleResponse>> ChangeUserRoleAsync(
            Guid userId,
            Role newRole,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return RoleErrors.UserNotFound;

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole is not null &&
                string.Equals(currentRole, newRole.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RoleErrors.SameRole;
            }

            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return RoleErrors.RemovalFailed;
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole.ToString());
            if (!addResult.Succeeded)
                return RoleErrors.AssignmentFailed;

            return new ChangeUserRoleResponse(
                user.Id,
                currentRole ?? "None",
                newRole.ToString());
        }

        public async Task<Result<AccountStatusResponse>> SuspendUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return AccountStatusErrors.UserNotFound;

            if (user.Status == AccountStatus.Suspended)
                return AccountStatusErrors.AlreadySuspended;

            user.Status = AccountStatus.Suspended;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return AccountStatusErrors.UpdateFailed;

            return new AccountStatusResponse(user.Id, user.Status.ToString());
        }

        public async Task<Result<AccountStatusResponse>> ReactivateUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return AccountStatusErrors.UserNotFound;

            if (user.Status == AccountStatus.Active)
                return AccountStatusErrors.AlreadyActive;

            user.Status = AccountStatus.Active;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return AccountStatusErrors.UpdateFailed;

            return new AccountStatusResponse(user.Id, user.Status.ToString());
        }

        public async Task<Result<ProfileDto>> GetProfileAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return ProfileErrors.UserNotFound;

            var roles = await _userManager.GetRolesAsync(user);

            return new ProfileDto(user.Id, user.Email!, user.Name, user.PhoneNumber, roles);
        }

        public async Task<Result<ProfileDto>> UpdateProfileAsync(
            Guid userId,
            string? name,
            string? phoneNumber,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                return ProfileErrors.UserNotFound;

            if (name is not null)
                user.Name = name;

            if (phoneNumber is not null)
                user.PhoneNumber = phoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return updateResult.Errors
                    .Select(e => Error.Validation("IdentityError", e.Description))
                    .ToList();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new ProfileDto(user.Id, user.Email!, user.Name, user.PhoneNumber, roles);
        }

        public async Task<Result<PasswordResetTokenDto>> GeneratePasswordResetTokenAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Error.NotFound("UserNotFound", "User not found.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new PasswordResetTokenDto(user.Email!, user.Name ?? user.Email!, token);
        }

        public async Task<Result<Success>> ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return PasswordResetErrors.InvalidOrExpiredToken; // don't reveal whether the email exists

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken"))
                    return PasswordResetErrors.InvalidOrExpiredToken;

                return result.Errors
                    .Select(e => Error.Validation("IdentityError", e.Description))
                    .ToList();
            }

            return Result.Success;
        }
    }
}