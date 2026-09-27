using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Dtos.Logout;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class LogoutCommandHandlerTests
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IUser _user;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _tokenProvider = Substitute.For<ITokenProvider>();
        _logger = Substitute.For<ILogger<LogoutCommandHandler>>();
        _user = Substitute.For<IUser>();

        _handler = new LogoutCommandHandler(
            _tokenProvider,
            _logger,
            _user);
    }

    [Fact]
    public async Task Handle_WhenRevocationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _user.Id.Returns(userId);

        _tokenProvider
            .RevokeTokenAsync(
                "refresh-token",
                userId,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        var command = new LogoutCommand(
            new LogoutRequest("refresh-token"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _tokenProvider.Received(1).RevokeTokenAsync(
            "refresh-token",
            userId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRevocationFails_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _user.Id.Returns(userId);

        var error = Error.Failure(
            "Token.RevocationFailed",
            "Token revocation failed.");

        _tokenProvider
            .RevokeTokenAsync(
                "refresh-token",
                userId,
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new LogoutCommand(
            new LogoutRequest("refresh-token"));

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