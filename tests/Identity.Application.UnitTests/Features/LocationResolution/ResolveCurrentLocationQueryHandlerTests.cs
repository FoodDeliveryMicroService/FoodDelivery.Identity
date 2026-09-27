using FluentValidation.TestHelper;
using Identity.Application.Features.LocationResolution.Dtos.ResolveLocation;
using Identity.Application.Features.LocationResolution.Queries.ResolveCurrentLocation;
using Xunit;

namespace Identity.Application.UnitTests.Features.LocationResolution;

public class ResolveCurrentLocationQueryValidatorTests
{
    private readonly ResolveCurrentLocationQueryValidator _validator = new();

    private static ResolveCurrentLocationQuery Query(double lat, double lon) =>
        new(new ResolveLocationRequest(lat, lon));

    // ---------- Valid cases ----------

    [Theory]
    [InlineData(0, 0)]
    [InlineData(30.05, 31.23)]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    public void Should_Not_Have_Errors_When_Coordinates_Are_Valid(double lat, double lon)
    {
        var result = _validator.TestValidate(Query(lat, lon));
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ---------- Latitude ----------

    [Theory]
    [InlineData(-90.0001)]
    [InlineData(90.0001)]
    [InlineData(-100)]
    [InlineData(200)]
    public void Should_Have_Error_When_Latitude_OutOfRange(double lat)
    {
        var result = _validator.TestValidate(Query(lat, 0));
        result.ShouldHaveValidationErrorFor(x => x.Request.Latitude);
    }

    // ---------- Longitude ----------

    [Theory]
    [InlineData(-180.0001)]
    [InlineData(180.0001)]
    [InlineData(-200)]
    [InlineData(300)]
    public void Should_Have_Error_When_Longitude_OutOfRange(double lon)
    {
        var result = _validator.TestValidate(Query(0, lon));
        result.ShouldHaveValidationErrorFor(x => x.Request.Longitude);
    }

    // ---------- Boundaries are inclusive ----------

    [Fact]
    public void Should_Not_Have_Errors_At_Exact_Boundaries()
    {
        _validator.TestValidate(Query(90, 180)).ShouldNotHaveAnyValidationErrors();
        _validator.TestValidate(Query(-90, -180)).ShouldNotHaveAnyValidationErrors();
    }
}