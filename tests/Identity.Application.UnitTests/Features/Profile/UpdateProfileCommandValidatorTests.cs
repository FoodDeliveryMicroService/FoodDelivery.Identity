using FluentValidation.TestHelper;
using Identity.Application.Features.Profile.Commands.UpdateProfile;
using Identity.Application.Features.Profile.Dtos.UpdateProfile;
using Xunit;

namespace Identity.Application.UnitTests.Features.Profile;

public class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateProfileCommand(Guid.Empty, new UpdateProfileRequest("Donia", "01000000000"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UserId);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), new UpdateProfileRequest(new string('a', 101), null));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Optional_Fields_Are_Null()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), new UpdateProfileRequest(null, null));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}