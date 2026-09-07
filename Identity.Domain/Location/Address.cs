using Identity.Domain.Common;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public sealed class Address : AuditableEntity
{
    public Guid CustomerId { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public Guid GovernorateId { get; private set; }
    public Guid CityId { get; private set; }

    public string Street { get; private set; } = string.Empty;
    public string BuildingNumber { get; private set; } = string.Empty;

    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string? Landmark { get; private set; }

    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    public bool IsDefault { get; private set; }

    private Address()
    {
    }

    private Address(
        Guid id,
        Guid customerId,
        string label,
        Guid governorateId,
        Guid cityId,
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string? landmark,
        double? latitude,
        double? longitude,
        bool isDefault)
        : base(id)
    {
        CustomerId = customerId;
        Label = label;
        GovernorateId = governorateId;
        CityId = cityId;
        Street = street;
        BuildingNumber = buildingNumber;
        Floor = floor;
        Apartment = apartment;
        Landmark = landmark;
        Latitude = latitude;
        Longitude = longitude;
        IsDefault = isDefault;
    }

    public static Result<Address> Create(
        Guid id,
        Guid customerId,
        string label,
        Guid governorateId,
        Guid cityId,
        string street,
        string buildingNumber,
        string? floor = null,
        string? apartment = null,
        string? landmark = null,
        double? latitude = null,
        double? longitude = null,
        bool isDefault = false)
    {
        if (customerId == Guid.Empty)
            return AddressErrors.InvalidCustomer;

        if (string.IsNullOrWhiteSpace(label))
            return AddressErrors.InvalidLabel;

        if (governorateId == Guid.Empty)
            return AddressErrors.InvalidGovernorate;

        if (cityId == Guid.Empty)
            return AddressErrors.InvalidCity;

        if (string.IsNullOrWhiteSpace(street))
            return AddressErrors.InvalidStreet;

        if (string.IsNullOrWhiteSpace(buildingNumber))
            return AddressErrors.InvalidBuildingNumber;

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            return AddressErrors.InvalidLatitude;

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            return AddressErrors.InvalidLongitude;

        var address = new Address(
            id,
            customerId,
            label.Trim(),
            governorateId,
            cityId,
            street.Trim(),
            buildingNumber.Trim(),
            floor?.Trim(),
            apartment?.Trim(),
            landmark?.Trim(),
            latitude,
            longitude,
            isDefault);

        return address;
    }

    public Result<Updated> Update(
        string label,
        Guid governorateId,
        Guid cityId,
        string street,
        string buildingNumber,
        string? floor = null,
        string? apartment = null,
        string? landmark = null,
        double? latitude = null,
        double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(label))
            return AddressErrors.InvalidLabel;

        if (governorateId == Guid.Empty)
            return AddressErrors.InvalidGovernorate;

        if (cityId == Guid.Empty)
            return AddressErrors.InvalidCity;

        if (string.IsNullOrWhiteSpace(street))
            return AddressErrors.InvalidStreet;

        if (string.IsNullOrWhiteSpace(buildingNumber))
            return AddressErrors.InvalidBuildingNumber;

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            return AddressErrors.InvalidLatitude;

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            return AddressErrors.InvalidLongitude;

        Label = label.Trim();
        GovernorateId = governorateId;
        CityId = cityId;
        Street = street.Trim();
        BuildingNumber = buildingNumber.Trim();
        Floor = floor?.Trim();
        Apartment = apartment?.Trim();
        Landmark = landmark?.Trim();
        Latitude = latitude;
        Longitude = longitude;

        return Result.Updated;
    }

    public Result<Updated> SetDefault()
    {
        if (IsDefault)
            return AddressErrors.AlreadyDefault;

        IsDefault = true;

        return Result.Updated;
    }

    public Result<Updated> RemoveDefault()
    {
        if (!IsDefault)
            return AddressErrors.NotDefault;

        IsDefault = false;

        return Result.Updated;
    }
}