using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.ConfirmEmail;
using Identity.Application.Features.Authentication.Dtos.Email;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class ConfirmEmailCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;
    private readonly ConfirmEmailCommandHandler _handler;

    public ConfirmEmailCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _logger = Substitute.For<ILogger<ConfirmEmailCommandHandler>>();

        _handler = new ConfirmEmailCommandHandler(
            _identityService,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenConfirmationSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";

        _identityService
            .ConfirmEmailAsync(
                userId,
                code,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                userId.ToString(),
                code));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _identityService.Received(1).ConfirmEmailAsync(
            userId,
            code,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIdIsInvalid_ShouldReturnError()
    {
        // Arrange
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                "invalid-guid",
                "123456"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);

        await _identityService
            .DidNotReceive()
            .ConfirmEmailAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfirmationFails_ShouldReturnIdentityServiceErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";

        var error = Error.Validation(
            "Email.InvalidCode",
            "The confirmation code is invalid.");

        _identityService
            .ConfirmEmailAsync(
                userId,
                code,
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                userId.ToString(),
                code));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(error.Code, result.TopError.Code);
    }
}