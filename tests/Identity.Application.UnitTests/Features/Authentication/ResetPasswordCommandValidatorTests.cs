using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.ResetPassword;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new();

    private static ResetPasswordCommand ValidCommand(
        string? email = null,
        string? token = null,
        string? password = null)
    {
        return new ResetPasswordCommand(
            new ResetPasswordRequest(
                email ?? "donia@example.com",
                token ?? "reset-token",
                password ?? "NewPassword123"));
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
    public void Empty_Token_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(token: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Token);
    }

    [Fact]
    public void Empty_NewPassword_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(password: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.NewPassword);
    }

    [Fact]
    public void Password_Shorter_Than_8_Characters_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(password: "1234567"));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.NewPassword);
    }
}