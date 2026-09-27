using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Identity.Application.Features.Identity;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RefreshTokenCommandHandlerTests
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _tokenProvider = Substitute.For<ITokenProvider>();
        _logger = Substitute.For<ILogger<RefreshTokenCommandHandler>>();

        _handler = new RefreshTokenCommandHandler(
            _tokenProvider,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenRefreshSucceeds_ShouldReturnNewTokens()
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var tokenResponse = new TokenResponse
        {
            AccessToken = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresOnUtc = expiresAt
        };

        _tokenProvider
            .RefreshTokenAsync(
                "old-refresh-token",
                Arg.Any<CancellationToken>())
            .Returns(tokenResponse);

        var command = new RefreshTokenCommand(
            new RefreshTokenRequest("old-refresh-token"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            "new-access-token",
            result.Value.AccessToken);
        Assert.Equal(
            "new-refresh-token",
            result.Value.RefreshToken);
        Assert.Equal(
            expiresAt,
            result.Value.ExpiresAt);

        await _tokenProvider.Received(1).RefreshTokenAsync(
            "old-refresh-token",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRefreshFails_ShouldReturnError()
    {
        // Arrange
        var error = Error.Validation(
            "Token.Invalid",
            "The refresh token is invalid.");

        _tokenProvider
            .RefreshTokenAsync(
                "invalid-token",
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new RefreshTokenCommand(
            new RefreshTokenRequest("invalid-token"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            error.Code,
            result.TopError.Code);
    }
}