using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.ResetPassword;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class ResetPasswordCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _logger = Substitute.For<ILogger<ResetPasswordCommandHandler>>();

        _handler = new ResetPasswordCommandHandler(
            _identityService,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenResetSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        _identityService
            .ResetPasswordAsync(
                "donia@example.com",
                "reset-token",
                "NewPassword123",
                Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        var command = new ResetPasswordCommand(
            new ResetPasswordRequest(
                "donia@example.com",
                "reset-token",
                "NewPassword123"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _identityService.Received(1).ResetPasswordAsync(
            "donia@example.com",
            "reset-token",
            "NewPassword123",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenResetFails_ShouldReturnError()
    {
        // Arrange
        var error = Error.Validation(
            "PasswordReset.InvalidOrExpiredToken",
            "The reset token is invalid or expired.");

        _identityService
            .ResetPasswordAsync(
                "donia@example.com",
                "invalid-token",
                "NewPassword123",
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new ResetPasswordCommand(
            new ResetPasswordRequest(
                "donia@example.com",
                "invalid-token",
                "NewPassword123"));

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