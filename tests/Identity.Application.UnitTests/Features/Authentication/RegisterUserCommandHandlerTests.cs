using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.RegisterUser;
using Identity.Application.Features.Authentication.Commands.SendConfirmationCode;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RegisterUserCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _mediator = Substitute.For<IMediator>();
        _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();

        _handler = new RegisterUserCommandHandler(
            _identityService,
            _mediator,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenUserCreationAndConfirmationSucceed_ShouldReturnUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var command = new RegisterUserCommand(
            new RegisterUserRequest(
                "Donia",
                "donia@example.com",
                "01000000000",
                "Str0ng!Pass",
                Role.Customer));

        var userDto = new AppUserDto(
            userId,
            "donia@example.com",
            new List<string>(),
            new List<System.Security.Claims.Claim>());

        _identityService
            .CreateUserAsync(
                "Donia",
                "donia@example.com",
                "01000000000",
                "Str0ng!Pass",
                Role.Customer.ToString(),
                Arg.Any<CancellationToken>())
            .Returns(userDto);

        _mediator
            .Send(
                Arg.Any<SendConfirmationCodeCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("donia@example.com", result.Value.Email);
        Assert.Equal("Donia", result.Value.Name);

        await _identityService.Received(1).CreateUserAsync(
            "Donia",
            "donia@example.com",
            "01000000000",
            "Str0ng!Pass",
            Role.Customer.ToString(),
            Arg.Any<CancellationToken>());

        await _mediator.Received(1).Send(
            Arg.Is<SendConfirmationCodeCommand>(
                x => x.Request.Email == "donia@example.com"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldReturnErrorAndNotSendConfirmationCode()
    {
        // Arrange
        var error = Error.Conflict(
            "DuplicateEmail",
            "Email is already registered.");

        _identityService
            .CreateUserAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new RegisterUserCommand(
            new RegisterUserRequest(
                "Donia",
                "donia@example.com",
                "01000000000",
                "Str0ng!Pass",
                Role.Customer));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            error.Code,
            result.TopError.Code);

        await _mediator
            .DidNotReceive()
            .Send(
                Arg.Any<SendConfirmationCodeCommand>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConfirmationCodeSendingFails_ShouldReturnError()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userDto = new AppUserDto(
            userId,
            "donia@example.com",
            new List<string>(),
            new List<System.Security.Claims.Claim>());

        var confirmationError = Error.Failure(
            "Email.CodeGenerationFailed",
            "Failed to generate confirmation code.");

        _identityService
            .CreateUserAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(userDto);

        _mediator
            .Send(
                Arg.Any<SendConfirmationCodeCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(confirmationError);

        var command = new RegisterUserCommand(
            new RegisterUserRequest(
                "Donia",
                "donia@example.com",
                "01000000000",
                "Str0ng!Pass",
                Role.Customer));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            confirmationError.Code,
            result.TopError.Code);
    }
}