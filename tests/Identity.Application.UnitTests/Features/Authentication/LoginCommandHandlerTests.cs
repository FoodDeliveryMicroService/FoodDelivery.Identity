using System.Security.Claims;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Dtos.Login;
using Identity.Application.Features.Identity;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _identityService = Substitute.For<IIdentityService>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();

        _handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);
    }

    [Fact]
    public async Task Handle_WhenAuthenticationSucceeds_ShouldGenerateTokensAndReturnLoginResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new AppUserDto(
            userId,
            "donia@example.com",
            new List<string> { "Customer" },
            new List<Claim>());

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var tokenResponse = new TokenResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            ExpiresOnUtc = expiresAt
        };

        _identityService
            .LoginAsync(
                "donia@example.com",
                "Str0ng!Pass",
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenProvider
            .GenerateJwtTokenAsync(
                user,
                Arg.Any<CancellationToken>())
            .Returns(tokenResponse);

        var command = new LoginCommand(
            new LoginRequest(
                "donia@example.com",
                "Str0ng!Pass"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal(expiresAt, result.Value.ExpiresAt);

        await _identityService.Received(1).LoginAsync(
            "donia@example.com",
            "Str0ng!Pass",
            Arg.Any<CancellationToken>());

        await _tokenProvider.Received(1).GenerateJwtTokenAsync(
            user,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAuthenticationFails_ShouldReturnErrorAndNotGenerateTokens()
    {
        // Arrange
        var error = Error.Validation(
            "Authentication.InvalidCredentials",
            "Invalid credentials.");

        _identityService
            .LoginAsync(
                "donia@example.com",
                "WrongPassword",
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new LoginCommand(
            new LoginRequest(
                "donia@example.com",
                "WrongPassword"));

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(
            error.Code,
            result.TopError.Code);

        await _tokenProvider
            .DidNotReceive()
            .GenerateJwtTokenAsync(
                Arg.Any<AppUserDto>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTokenGenerationFails_ShouldReturnError()
    {
        // Arrange
        var user = new AppUserDto(
            Guid.NewGuid(),
            "donia@example.com",
            new List<string> { "Customer" },
            new List<Claim>());

        var error = Error.Failure(
            "Token.GenerationFailed",
            "Token generation failed.");

        _identityService
            .LoginAsync(
                "donia@example.com",
                "Str0ng!Pass",
                Arg.Any<CancellationToken>())
            .Returns(user);

        _tokenProvider
            .GenerateJwtTokenAsync(
                user,
                Arg.Any<CancellationToken>())
            .Returns(error);

        var command = new LoginCommand(
            new LoginRequest(
                "donia@example.com",
                "Str0ng!Pass"));

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