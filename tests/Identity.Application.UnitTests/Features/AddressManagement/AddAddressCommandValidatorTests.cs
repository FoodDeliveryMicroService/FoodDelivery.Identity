using FluentValidation.TestHelper;
using Identity.Application.Features.AddressManagement.Commands.AddAddress;
using Identity.Application.Features.AddressManagement.Dtos.AddAddress;
using Xunit;

namespace Identity.Application.UnitTests.Features.AddressManagement;

public class AddAddressCommandValidatorTests
{
    private readonly AddAddressCommandValidator _validator = new();

    private static AddAddressCommand ValidCommand(
        string? label = null,
        Guid? governorateId = null,
        Guid? cityId = null,
        string? street = null,
        string? buildingNumber = null,
        double? latitude = null,
        double? longitude = null) =>
        new(new AddAddressRequest(
            label ?? "Home",
            governorateId ?? Guid.NewGuid(),
            cityId ?? Guid.NewGuid(),
            street ?? "St",
            buildingNumber ?? "1",
            null, null, null,
            latitude, longitude,
            false));

    [Fact]
    public void Should_Have_Error_When_Label_Is_Empty()
    {
        var command = ValidCommand(label: string.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.Label);
    }

    [Fact]
    public void Should_Have_Error_When_GovernorateId_Is_Empty()
    {
        var command = ValidCommand(governorateId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.GovernorateId);
    }

    [Fact]
    public void Should_Have_Error_When_CityId_Is_Empty()
    {
        var command = ValidCommand(cityId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.CityId);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Should_Have_Error_When_Latitude_OutOfRange(double latitude)
    {
        var command = ValidCommand(latitude: latitude);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Request.Latitude);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        var command = ValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
