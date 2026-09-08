using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Email;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Application.Features.Authentication.Dtos.Email;

namespace Identity.Infrastructure.Services
{
    public class IdentityService(
    UserManager<AppUser> userManager,
    AppDbContext _context,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService) : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
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
            {
                return false;
            }

            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

            var result = await _authorizationService.AuthorizeAsync(principal, policyName!);

            return result.Succeeded;
        }

        public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return Error.NotFound("User_Not_Found", $"User with email {UtilityService.MaskEmail(email)} not found");
            }

            if (!user.EmailConfirmed)
            {
                return Error.Conflict("Email_Not_Confirmed", $"email '{UtilityService.MaskEmail(email)}' not confirmed");
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return Error.Conflict("Invalid_Login_Attempt", "Email / Password are incorrect");
            }

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
        // Identity.Infrastructure/Services/IdentityService.cs

        // Identity.Infrastructure/Services/IdentityService.cs

        public async Task<Result<AppUserDto>> CreateUserAsync(
            string name,
            string email,
            string phoneNumber,
            string password,
            string role,
            CancellationToken cancellationToken = default)
        {
            // 1. Check if the email is already taken
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser is not null)
            {
                // Return a single Conflict error – implicit conversion to Result<AppUserDto>
                return Error.Conflict("DuplicateEmail", $"Email '{UtilityService.MaskEmail(email)}' is already registered.");
            }

            // 2. Create a new user entity
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                Name = name,
                PhoneNumber = phoneNumber
            };

            // 3. Create the user with the given password
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                // Convert each Identity error to our Error type
                var errors = createResult.Errors
                    .Select(e => Error.Validation("IdentityError", e.Description))
                    .ToList();

                // Return the list of errors – implicit conversion to Result<AppUserDto>
                return errors;
            }

            // 4. Assign the requested role to the user
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                // Rollback: delete the created user
                await _userManager.DeleteAsync(user);
                return Error.Validation("RoleAssignmentFailed", $"Failed to assign role '{role}'.");
            }

            // 5. Success: return the user DTO – implicit conversion to Result<AppUserDto>
            return new AppUserDto(
                user.Id,
                user.Email!,
                await _userManager.GetRolesAsync(user),
                await _userManager.GetClaimsAsync(user)
            );
        }
        // Add these new methods to your existing IdentityService class

        public async Task<Result<EmailConfirmationDto>> GenerateConfirmationCodeAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Error.NotFound("UserNotFound", "User not found.");

            if (user.EmailConfirmed)
                return EmailConfirmationErrors.AlreadyConfirmed;

            // Generate a random 6-digit code
            var code = new Random().Next(100000, 999999).ToString();

            // Save the confirmation code
            var confirmation = new EmailConfirmation(
                userId,
                code,
                DateTime.UtcNow.AddMinutes(15)
            );

            await _context.EmailConfirmations.AddAsync(confirmation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new EmailConfirmationDto(
                confirmation.Id,
                confirmation.Code,
                confirmation.ExpiresAt
            );
        }

        public async Task<Result<bool>> ConfirmEmailAsync(
            string userId,
            string code,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Error.NotFound("UserNotFound", "User not found.");

            if (user.EmailConfirmed)
                return EmailConfirmationErrors.AlreadyConfirmed;

            // Find the latest unused confirmation code
            var confirmation = await _context.EmailConfirmations
                .Where(c => c.UserId == userId && !c.IsUsed)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (confirmation is null)
                return EmailConfirmationErrors.InvalidCode;

            // Validate the code
            if (confirmation.Code != code)
            {
                confirmation.IncrementAttempt();
                await _context.SaveChangesAsync(cancellationToken);
                return EmailConfirmationErrors.InvalidCode;
            }

            // Use the code (validates expiration internally)
            var useResult = confirmation.Use();
            if (useResult.IsError)
                return useResult.Errors;

            // Confirm the email
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        public async Task<Result<AppUserDto>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Error.NotFound("UserNotFound", $"User with email '{UtilityService.MaskEmail(email)}' not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AppUserDto(user.Id, user.Email!, roles, claims);
        }
        public async Task<bool> IsEmailConfirmedAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return false; // or throw, but returning false is safe for the check

            return user.EmailConfirmed;
        }
    }
}
