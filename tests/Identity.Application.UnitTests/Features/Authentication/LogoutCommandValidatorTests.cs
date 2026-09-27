using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Dtos.Logout;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class LogoutCommandValidatorTests
{
    private readonly LogoutCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        var command = new LogoutCommand(
            new LogoutRequest("refresh-token"));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_RefreshToken_Should_Fail()
    {
        var command = new LogoutCommand(
            new LogoutRequest(string.Empty));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(
            x => x.Request.RefreshToken);
    }
}