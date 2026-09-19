namespace Identity.Application.Features.GeographicLookup.Dtos;

public sealed record CityDto(Guid Id, Guid GovernorateId, string NameAr, string NameEn);