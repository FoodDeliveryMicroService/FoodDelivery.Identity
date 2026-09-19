using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Common.Results;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutCommandHandler(
    ITokenProvider tokenProvider,
    ILogger<LogoutCommandHandler> logger,
    IUser user)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        logger.LogInformation("Logout requested");

        // 1. Revoke the refresh token
        var revokeResult = await tokenProvider.RevokeTokenAsync(
            request.RefreshToken,user.Id,
            cancellationToken);

        if (revokeResult.IsError)
            return revokeResult.Errors;

        logger.LogInformation("User logged out successfully");

        return Result.Success;
    }
}