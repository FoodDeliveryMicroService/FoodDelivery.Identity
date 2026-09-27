using Identity.Domain.Location;
using Identity.Tests.Common.Location;
using Xunit;

namespace Identity.Domain.UnitTests.Location;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var customerId = Guid.NewGuid();
        var governorateId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

        var result = AddressFactory.CreateAddress(
            customerId: customerId,
            label: "Work",
            governorateId: governorateId,
            cityId: cityId,
            street: "Tahrir St.",
            buildingNumber: "5",
            floor: "3",
            apartment: "12",
            latitude: 30.05,
            longitude: 31.23);

        Assert.True(result.IsSuccess);

        var address = result.Value;
        Assert.Equal(customerId, address.CustomerId);
        Assert.Equal("Work", address.Label);
        Assert.Equal(governorateId, address.GovernorateId);
        Assert.Equal(cityId, address.CityId);
        Assert.Equal("Tahrir St.", address.Street);
        Assert.Equal("5", address.BuildingNumber);
        Assert.False(address.IsDefault);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldFail()
    {
        var result = Address.Create(
            Guid.NewGuid(), Guid.Empty, "Home", Guid.NewGuid(), Guid.NewGuid(), "St", "1");

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidCustomer.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidLabel_ShouldFail(string label)
    {
        var result = AddressFactory.CreateAddress(label: label);

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidLabel.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_WithEmptyGovernorateId_ShouldFail()
    {
        var result = Address.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Home", Guid.Empty, Guid.NewGuid(), "St", "1");

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidGovernorate.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_WithEmptyCityId_ShouldFail()
    {
        var result = Address.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Home", Guid.NewGuid(), Guid.Empty, "St", "1");

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidCity.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStreet_ShouldFail(string street)
    {
        var result = AddressFactory.CreateAddress(street: street);

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidStreet.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidBuildingNumber_ShouldFail(string buildingNumber)
    {
        var result = AddressFactory.CreateAddress(buildingNumber: buildingNumber);

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidBuildingNumber.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Create_WithInvalidLatitude_ShouldFail(double latitude)
    {
        var result = AddressFactory.CreateAddress(latitude: latitude);

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidLatitude.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Create_WithInvalidLongitude_ShouldFail(double longitude)
    {
        var result = AddressFactory.CreateAddress(longitude: longitude);

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidLongitude.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_WithValidData_ShouldSucceed()
    {
        var address = AddressFactory.CreateAddress().Value;

        var result = address.Update(
            "New Label", Guid.NewGuid(), Guid.NewGuid(), "New St.", "9", "1", "2", "Near mosque", 30.1, 31.1);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Label", address.Label);
        Assert.Equal("New St.", address.Street);
        Assert.Equal("9", address.BuildingNumber);
    }

    [Fact]
    public void Update_WithInvalidLabel_ShouldFail()
    {
        var address = AddressFactory.CreateAddress().Value;

        var result = address.Update(" ", Guid.NewGuid(), Guid.NewGuid(), "St", "1");

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.InvalidLabel.Code, result.TopError.Code);
    }

    [Fact]
    public void SetDefault_WhenNotDefault_ShouldSucceed()
    {
        var address = AddressFactory.CreateAddress(isDefault: false).Value;

        var result = address.SetDefault();

        Assert.True(result.IsSuccess);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public void SetDefault_WhenAlreadyDefault_ShouldFail()
    {
        var address = AddressFactory.CreateAddress(isDefault: true).Value;

        var result = address.SetDefault();

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.AlreadyDefault.Code, result.TopError.Code);
    }

    [Fact]
    public void RemoveDefault_WhenDefault_ShouldSucceed()
    {
        var address = AddressFactory.CreateAddress(isDefault: true).Value;

        var result = address.RemoveDefault();

        Assert.True(result.IsSuccess);
        Assert.False(address.IsDefault);
    }

    [Fact]
    public void RemoveDefault_WhenNotDefault_ShouldFail()
    {
        var address = AddressFactory.CreateAddress(isDefault: false).Value;

        var result = address.RemoveDefault();

        Assert.True(result.IsError);
        Assert.Equal(AddressErrors.NotDefault.Code, result.TopError.Code);
    }
}
