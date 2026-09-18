namespace Identity.Application.Features.LocationResolution.Dtos.ResolveLocation;

public sealed record ResolveLocationResponse(
    double Latitude,
    double Longitude,
    Guid? GovernorateId,
    string? GovernorateNameAr,
    string? GovernorateNameEn,
    Guid? CityId,
    string? CityNameAr,
    string? CityNameEn,
    bool IsFullyResolved);