using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.RequestPasswordReset;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RequestPasswordResetCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;
    private readonly RequestPasswordResetCommandHandler _handler;

    public RequestPasswordResetCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _emailService = Substitute.For<IEmailService>();
        _logger = Substitute.For<ILogger<RequestPasswordResetCommandHandler>>();

        _handler = new RequestPasswordResetCommandHandler(
            _identityService,
            _emailService,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ShouldGenerateTokenAndSendEmail()
    {
        // Arrange
        var dto = new PasswordResetTokenDto(
            "donia@example.com",
            "Donia",
            "reset-token");

        _identityService
            .GeneratePasswordResetTokenAsync(
                "donia@example.com",
                Arg.Any<CancellationToken>())
            .Returns(dto);

        _emailService
            .SendPasswordResetEmailAsync(
                "donia@example.com",
                "Donia",
                "reset-token",
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                "donia@example.com"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _identityService.Received(1)
            .GeneratePasswordResetTokenAsync(
                "donia@example.com",
                Arg.Any<CancellationToken>());

        await _emailService.Received(1)
            .SendPasswordResetEmailAsync(
                "donia@example.com",
                "Donia",
                "reset-token",
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailDoesNotExist_ShouldStillReturnSuccessAndNotSendEmail()
    {
        // Arrange
        var error = Error.NotFound(
            "UserNotFound",
            "User not found.");

        _identityService
            .GeneratePasswordResetTokenAsync(
                "unknown@example.com",
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                "unknown@example.com"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _emailService
            .DidNotReceive()
            .SendPasswordResetEmailAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailSendingFails_ShouldStillReturnSuccess()
    {
        // Arrange
        var dto = new PasswordResetTokenDto(
            "donia@example.com",
            "Donia",
            "reset-token");

        _identityService
            .GeneratePasswordResetTokenAsync(
                "donia@example.com",
                Arg.Any<CancellationToken>())
            .Returns(dto);

        _emailService
            .SendPasswordResetEmailAsync(
                "donia@example.com",
                "Donia",
                "reset-token",
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                "donia@example.com"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}