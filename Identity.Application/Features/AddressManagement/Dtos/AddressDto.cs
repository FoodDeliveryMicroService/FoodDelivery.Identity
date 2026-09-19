namespace Identity.Application.Features.AddressManagement.Dtos;

public sealed record AddressDto(
    Guid Id,
    string Label,
    Guid GovernorateId,
    string GovernorateNameAr,
    string GovernorateNameEn,
    Guid CityId,
    string CityNameAr,
    string CityNameEn,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string? Landmark,
    double? Latitude,
    double? Longitude,
    bool IsDefault);