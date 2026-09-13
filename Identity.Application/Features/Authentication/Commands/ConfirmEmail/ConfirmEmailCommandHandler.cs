using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using Identity.Domain.Email;

namespace Identity.Application.Features.Authentication.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler(
    IIdentityService identityService,
    ILogger<ConfirmEmailCommandHandler> logger)
    : IRequestHandler<ConfirmEmailCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Parse the registration token — it's a Guid represented as string in the request
        if (!Guid.TryParse(request.userId, out var registrationToken))
            return EmailConfirmationErrors.InvalidCode;

        logger.LogInformation("Confirming email for registration token: {Token}", registrationToken);

        // Single call — no more email lookup + separate ID lookup (fixes ARC-03)
        var confirmResult = await identityService.ConfirmEmailAsync(
            registrationToken,
            request.Code,
            cancellationToken);

        if (confirmResult.IsError)
            return confirmResult.Errors;

        logger.LogInformation("Email confirmed successfully for registration token: {Token}", registrationToken);

        return Result.Success;
    }
}