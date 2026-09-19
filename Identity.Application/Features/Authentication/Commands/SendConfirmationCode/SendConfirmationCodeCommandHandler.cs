using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using Identity.Domain.Email;

namespace Identity.Application.Features.Authentication.Commands.SendConfirmationCode;

public sealed class SendConfirmationCodeCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    ILogger<SendConfirmationCodeCommandHandler> logger)
    : IRequestHandler<SendConfirmationCodeCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
    SendConfirmationCodeCommand command,
    CancellationToken cancellationToken)
    {
        var request = command.Request;

        logger.LogInformation("Sending confirmation code to: {Email}", request.Email);

        // 1. Check if email is already confirmed
        var isConfirmed = await identityService.IsEmailConfirmedAsync(request.Email, cancellationToken);
        if (isConfirmed)
            return EmailConfirmationErrors.AlreadyConfirmed;

        // 2. Get user details (UserId and Name)
        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsError)
            return userResult.Errors;

        var user = userResult.Value;

        // 3. Get the user's Name separately
        var userName = await identityService.GetUserNameAsync(userResult.Value.UserId.ToString());
        if (string.IsNullOrEmpty(userName))
            userName = "User"; // fallback

        // 4. Generate confirmation code
        var codeResult = await identityService.GenerateConfirmationCodeAsync(
            user.UserId.ToString(),
            cancellationToken);

        if (codeResult.IsError)
            return codeResult.Errors;

        var confirmationData = codeResult.Value;

        // 5. Send email
        var emailSent = await emailService.SendConfirmationCodeAsync(
            user.Email,
            userName, 
            confirmationData.Code,
            cancellationToken);

        if (!emailSent)
            return EmailConfirmationErrors.CodeGenerationFailed;

        logger.LogInformation("Confirmation code sent to: {Email}", request.Email);

        return Result.Success;
    }
}