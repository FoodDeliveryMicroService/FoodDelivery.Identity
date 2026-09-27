using FluentValidation.TestHelper;
using Identity.Application.Features.Profile.Commands.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.AdminUpdateUser;
using Xunit;

namespace Identity.Application.UnitTests.Features.Profile;

public class AdminUpdateUserCommandValidatorTests
{
    private readonly AdminUpdateUserCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_TargetUserId_Is_Empty()
    {
        var command = new AdminUpdateUserCommand(Guid.Empty, new AdminUpdateUserRequest("Donia", null));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TargetUserId);
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Exceeds_20_Characters()
    {
        var command = new AdminUpdateUserCommand(Guid.NewGuid(), new AdminUpdateUserRequest(null, new string('1', 21)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.PhoneNumber);
    }
}