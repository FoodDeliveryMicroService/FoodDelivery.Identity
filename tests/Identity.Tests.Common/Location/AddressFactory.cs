using Identity.Domain.Common.Results;
using Identity.Domain.Location;

namespace Identity.Tests.Common.Location;

public static class AddressFactory
{
    public static Result<Address> CreateAddress(
        Guid? id = null,
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
        double? longitude = null,
        bool isDefault = false)
    {
        return Address.Create(
            id ?? Guid.NewGuid(),
            customerId ?? Guid.NewGuid(),
            label ?? "Home",
            governorateId ?? Guid.NewGuid(),
            cityId ?? Guid.NewGuid(),
            street ?? "123 Some Street",
            buildingNumber ?? "12",
            floor,
            apartment,
            landmark,
            latitude,
            longitude,
            isDefault);
    }
}
