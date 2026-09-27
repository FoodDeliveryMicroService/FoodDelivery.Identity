using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Dtos.RefreshToken;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        var command = new RefreshTokenCommand(
            new RefreshTokenRequest("refresh-token"));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_RefreshToken_Should_Fail()
    {
        var command = new RefreshTokenCommand(
            new RefreshTokenRequest(string.Empty));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Request.RefreshToken);
    }
}