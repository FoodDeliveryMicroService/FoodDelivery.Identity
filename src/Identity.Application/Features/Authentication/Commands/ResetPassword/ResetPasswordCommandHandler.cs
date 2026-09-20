using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        var result = await identityService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        logger.LogInformation("Password reset completed for {Email}.", request.Email);

        return Result.Success;
    }
}