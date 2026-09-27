using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.RequestPasswordReset;
using Identity.Application.Features.Authentication.Dtos.ResetPassword;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RequestPasswordResetCommandValidatorTests
{
    private readonly RequestPasswordResetCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        // Arrange
        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                "donia@example.com"));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Should_Fail()
    {
        // Arrange
        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                string.Empty));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            x => x.Request.Email);
    }

    [Fact]
    public void Invalid_Email_Should_Fail()
    {
        // Arrange
        var command = new RequestPasswordResetCommand(
            new RequestPasswordResetRequest(
                "invalid-email"));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(
            x => x.Request.Email);
    }
}