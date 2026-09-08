using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;

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

        logger.LogInformation("Confirming email for: {Email}", request.Email);

        // 1. Find the user by email
        var userResult = await identityService.GetUserByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsError)
            return userResult.Errors;

        var user = userResult.Value;

        // 2. Confirm the email with the code
        var confirmResult = await identityService.ConfirmEmailAsync(
            user.UserId.ToString(),
            request.Code,
            cancellationToken);

        if (confirmResult.IsError)
            return confirmResult.Errors;

        logger.LogInformation("Email confirmed successfully for: {Email}", request.Email);

        return Result.Success;
    }
}