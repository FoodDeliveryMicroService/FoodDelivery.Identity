using FluentValidation.TestHelper;
using Identity.Application.Features.AddressManagement.Commands.UpdateAddress;
using Identity.Application.Features.AddressManagement.Dtos.UpdateAddress;
using Xunit;

namespace Identity.Application.UnitTests.Features.AddressManagement;

public class UpdateAddressCommandValidatorTests
{
    private readonly UpdateAddressCommandValidator _validator = new();

    private static UpdateAddressCommand ValidCommand(
        Guid? addressId = null,
        Guid? customerId = null,
        string? label = null,
        Guid? governorateId = null,
        Guid? cityId = null,
        string? street = null,
        string? buildingNumber = null,
        string? floor = null,
        string? apartment = null,
        string? landmark = null,
        double? latitude = null,
        double? longitude = null) =>
        new(
            addressId ?? Guid.NewGuid(),
            customerId ?? Guid.NewGuid(),
            new UpdateAddressRequest(
                label ?? "Home",
                governorateId ?? Guid.NewGuid(),
                cityId ?? Guid.NewGuid(),
                street ?? "Some Street",
                buildingNumber ?? "1",
                floor,
                apartment,
                landmark,
                latitude,
                longitude));

    // ---------- AddressId ----------

    [Fact]
    public void Should_Have_Error_When_AddressId_Is_Empty()
    {
        var result = _validator.TestValidate(ValidCommand(addressId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.AddressId);
    }

    // ---------- CustomerId ----------

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var result = _validator.TestValidate(ValidCommand(customerId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.CustomerId);
    }

    // ---------- Label ----------

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_Label_Is_Empty(string label)
    {
        var result = _validator.TestValidate(ValidCommand(label: label));
        result.ShouldHaveValidationErrorFor(c => c.Request.Label);
    }

    [Fact]
    public void Should_Have_Error_When_Label_Exceeds_50_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(label: new string('a', 51)));
        result.ShouldHaveValidationErrorFor(c => c.Request.Label);
    }

    // ---------- Street ----------

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_Street_Is_Empty(string street)
    {
        var result = _validator.TestValidate(ValidCommand(street: street));
        result.ShouldHaveValidationErrorFor(c => c.Request.Street);
    }

    [Fact]
    public void Should_Have_Error_When_Street_Exceeds_200_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(street: new string('s', 201)));
        result.ShouldHaveValidationErrorFor(c => c.Request.Street);
    }

    // ---------- BuildingNumber ----------

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Have_Error_When_BuildingNumber_Is_Empty(string buildingNumber)
    {
        var result = _validator.TestValidate(ValidCommand(buildingNumber: buildingNumber));
        result.ShouldHaveValidationErrorFor(c => c.Request.BuildingNumber);
    }

    [Fact]
    public void Should_Have_Error_When_BuildingNumber_Exceeds_50_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(buildingNumber: new string('1', 51)));
        result.ShouldHaveValidationErrorFor(c => c.Request.BuildingNumber);
    }

    // ---------- Geographic Ids ----------

    [Fact]
    public void Should_Have_Error_When_GovernorateId_Is_Empty()
    {
        var result = _validator.TestValidate(ValidCommand(governorateId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Request.GovernorateId);
    }

    [Fact]
    public void Should_Have_Error_When_CityId_Is_Empty()
    {
        var result = _validator.TestValidate(ValidCommand(cityId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(c => c.Request.CityId);
    }

    // ---------- Optional Fields ----------

    [Fact]
    public void Should_Have_Error_When_Floor_Exceeds_20_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(floor: new string('f', 21)));
        result.ShouldHaveValidationErrorFor(c => c.Request.Floor);
    }

    [Fact]
    public void Should_Have_Error_When_Apartment_Exceeds_50_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(apartment: new string('a', 51)));
        result.ShouldHaveValidationErrorFor(c => c.Request.Apartment);
    }

    [Fact]
    public void Should_Have_Error_When_Landmark_Exceeds_200_Chars()
    {
        var result = _validator.TestValidate(ValidCommand(landmark: new string('l', 201)));
        result.ShouldHaveValidationErrorFor(c => c.Request.Landmark);
    }

    // ---------- Coordinates ----------

    [Theory]
    [InlineData(-90.01)]
    [InlineData(90.01)]
    public void Should_Have_Error_When_Latitude_OutOfRange(double latitude)
    {
        var result = _validator.TestValidate(ValidCommand(latitude: latitude));
        result.ShouldHaveValidationErrorFor(c => c.Request.Latitude);
    }

    [Theory]
    [InlineData(-180.01)]
    [InlineData(180.01)]
    public void Should_Have_Error_When_Longitude_OutOfRange(double longitude)
    {
        var result = _validator.TestValidate(ValidCommand(longitude: longitude));
        result.ShouldHaveValidationErrorFor(c => c.Request.Longitude);
    }

    // ---------- Happy Paths ----------

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Optional_Fields_Are_Null()
    {
        var result = _validator.TestValidate(ValidCommand(
            floor: null, apartment: null, landmark: null, latitude: null, longitude: null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Errors_When_Coordinates_At_Boundaries()
    {
        var result = _validator.TestValidate(ValidCommand(latitude: 90, longitude: 180));
        result.ShouldNotHaveAnyValidationErrors();

        var result2 = _validator.TestValidate(ValidCommand(latitude: -90, longitude: -180));
        result2.ShouldNotHaveAnyValidationErrors();
    }
}