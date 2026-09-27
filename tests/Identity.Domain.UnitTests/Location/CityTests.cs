using Identity.Domain.Location;
using Identity.Tests.Common.Location;
using Xunit;

namespace Identity.Domain.UnitTests.Location;

public class CityTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var id = Guid.NewGuid();
        var governorateId = Guid.NewGuid();

        var result = CityFactory.CreateCity(id: id, governorateId: governorateId, nameAr: "الزقازيق", nameEn: "Zagazig");

        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(governorateId, result.Value.GovernorateId);
        Assert.Equal("الزقازيق", result.Value.NameAr);
        Assert.Equal("Zagazig", result.Value.NameEn);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        var result = City.Create(Guid.Empty, Guid.NewGuid(), "الزقازيق", "Zagazig");

        Assert.True(result.IsError);
        Assert.Equal(CityErrors.InvalidId.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_WithEmptyGovernorateId_ShouldFail()
    {
        var result = City.Create(Guid.NewGuid(), Guid.Empty, "الزقازيق", "Zagazig");

        Assert.True(result.IsError);
        Assert.Equal(CityErrors.InvalidGovernorate.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidArabicName_ShouldFail(string nameAr)
    {
        var result = CityFactory.CreateCity(nameAr: nameAr);

        Assert.True(result.IsError);
        Assert.Equal(CityErrors.InvalidArabicName.Code, result.TopError.Code);
    }

    [Fact]
    public void ChangeGovernorate_WithValidId_ShouldSucceed()
    {
        var city = CityFactory.CreateCity().Value;
        var newGovernorateId = Guid.NewGuid();

        var result = city.ChangeGovernorate(newGovernorateId);

        Assert.True(result.IsSuccess);
        Assert.Equal(newGovernorateId, city.GovernorateId);
    }

    [Fact]
    public void ChangeGovernorate_WithEmptyId_ShouldFail()
    {
        var city = CityFactory.CreateCity().Value;

        var result = city.ChangeGovernorate(Guid.Empty);

        Assert.True(result.IsError);
        Assert.Equal(CityErrors.InvalidGovernorate.Code, result.TopError.Code);
    }
}
