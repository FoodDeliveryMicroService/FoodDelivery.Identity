using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.RegisterUser;
using Identity.Application.Features.Authentication.Dtos.RegisterUser;
using Identity.Domain.Identity;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private static RegisterUserCommand ValidCommand(
        string? name = null,
        string? email = null,
        string? phoneNumber = null,
        string? password = null,
        Role? role = null)
    {
        return new RegisterUserCommand(
            new RegisterUserRequest(
                name ?? "Donia",
                email ?? "donia@example.com",
                phoneNumber ?? "01000000000",
                password ?? "Str0ng!Pass",
                role ?? Role.Customer));
    }

    [Fact]
    public void Valid_Command_Should_Pass()
    {
        var result = _validator.TestValidate(
            ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Name_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(name: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Name);
    }

    [Fact]
    public void Name_Over_100_Characters_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(name: new string('A', 101)));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Name);
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
    public void Empty_PhoneNumber_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(phoneNumber: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.PhoneNumber);
    }

    [Fact]
    public void PhoneNumber_Over_20_Characters_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(phoneNumber: new string('1', 21)));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.PhoneNumber);
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar1")]
    public void Weak_Password_Should_Fail(string password)
    {
        var result = _validator.TestValidate(
            ValidCommand(password: password));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Password);
    }

    [Fact]
    public void Empty_Password_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(password: string.Empty));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Password);
    }

    [Fact]
    public void Admin_Role_Should_Fail()
    {
        var result = _validator.TestValidate(
            ValidCommand(role: Role.Admin));

        result.ShouldHaveValidationErrorFor(
            x => x.Request.Role);
    }

    [Theory]
    [InlineData(Role.Customer)]
    [InlineData(Role.RestaurantOwner)]
    public void Allowed_Role_Should_Pass(Role role)
    {
        var result = _validator.TestValidate(
            ValidCommand(role: role));

        result.ShouldNotHaveValidationErrorFor(
            x => x.Request.Role);
    }
}