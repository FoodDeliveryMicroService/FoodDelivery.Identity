namespace Identity.Application.Features.AddressManagement.Dtos.UpdateAddress;

public sealed record UpdateAddressRequest(
    string Label,
    Guid GovernorateId,
    Guid CityId,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string? Landmark,
    double? Latitude,
    double? Longitude);