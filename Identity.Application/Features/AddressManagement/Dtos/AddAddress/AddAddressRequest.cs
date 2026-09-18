namespace Identity.Application.Features.AddressManagement.Dtos.AddAddress;

public sealed record AddAddressRequest(
    string Label,
    Guid GovernorateId,
    Guid CityId,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string? Landmark,
    double? Latitude,
    double? Longitude,
    bool IsDefault);