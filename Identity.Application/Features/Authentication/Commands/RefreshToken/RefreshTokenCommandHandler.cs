using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    ITokenProvider tokenProvider,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<
        RefreshTokenCommand,
        Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Refresh token exchange requested.");

        var refreshResult = await tokenProvider.RefreshTokenAsync(
            command.Request.RefreshToken,
            cancellationToken);

        if (refreshResult.IsError)
        {
            logger.LogWarning(
                "Refresh token exchange failed.");

            return refreshResult.Errors;
        }

        var tokens = refreshResult.Value;

        logger.LogInformation(
            "Refresh token exchanged successfully.");

        return new RefreshTokenResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresOnUtc);
    }
}