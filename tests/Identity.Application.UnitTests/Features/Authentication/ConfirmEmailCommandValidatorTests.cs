using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.ConfirmEmail;
using Identity.Application.Features.Authentication.Dtos.Email;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class ConfirmEmailCommandValidatorTests
{
    private readonly ConfirmEmailCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        // Arrange
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                Guid.NewGuid().ToString(),
                "123456"));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_RegistrationToken_Should_Fail()
    {
        // Arrange
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                string.Empty,
                "123456"));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.userId);
    }

    [Fact]
    public void Empty_Code_Should_Fail()
    {
        // Arrange
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                Guid.NewGuid().ToString(),
                string.Empty));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Code);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void Invalid_Code_Length_Should_Fail(string code)
    {
        // Arrange
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(
                Guid.NewGuid().ToString(),
                code));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Code);
    }
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Empty_Or_Whitespace_Code_Should_Fail(string? code)
    {
        var command = new ConfirmEmailCommand(
            new ConfirmEmailRequest(Guid.NewGuid().ToString(), code!));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Code);
    }
}