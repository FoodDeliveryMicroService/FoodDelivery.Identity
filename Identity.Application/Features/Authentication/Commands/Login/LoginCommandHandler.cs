using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        logger.LogInformation("Login attempt for: {Email}", request.Email);

        // 1. Authenticate the user
        var authResult = await identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (authResult.IsError)
            return authResult.Errors;

        var user = authResult.Value;

        // 2. Generate JWT tokens
        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(user, cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        var tokens = tokenResult.Value;

        logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return new LoginResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresOnUtc
        );
    }
}