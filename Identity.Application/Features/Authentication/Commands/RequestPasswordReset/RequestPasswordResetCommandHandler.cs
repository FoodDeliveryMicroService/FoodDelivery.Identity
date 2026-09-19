using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    ILogger<RequestPasswordResetCommandHandler> logger)
    : IRequestHandler<RequestPasswordResetCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Request.Email;

        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(email, cancellationToken);

        if (tokenResult.IsSuccess)
        {
            var dto = tokenResult.Value;

            var emailSent = await emailService.SendPasswordResetEmailAsync(
                dto.Email,
                dto.UserName,
                dto.Token,
                cancellationToken);

            if (!emailSent)
            {
                logger.LogError(
                    "Password reset was requested but the email could not be sent to {Email}.",
                    email);
            }
        }
        else
        {
            // Don't reveal whether the email is registered - log internally only
            logger.LogInformation("Password reset requested for an unregistered email.");
        }

        // Always return a generic success response to prevent user enumeration
        return Result.Success;
    }
}