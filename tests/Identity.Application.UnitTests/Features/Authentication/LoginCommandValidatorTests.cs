using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Dtos.Login;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    private static LoginCommand ValidCommand(
        string? email = null,
        string? password = null)
    {
        return new LoginCommand(
            new LoginRequest(
                email ?? "donia@example.com",
                password ?? "Str0ng!Pass"));
    }

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        var result = _validator.TestValidate(
            ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(email: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Email);
    }

    [Fact]
    public void Invalid_Email_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(email: "invalid-email"));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Email);
    }

    [Fact]
    public void Empty_Password_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(password: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Password);
    }
}