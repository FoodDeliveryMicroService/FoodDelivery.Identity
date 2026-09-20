using Identity.Application.Common.Interfaces;
using Identity.Application.Features.GeographicLookup.Dtos;
using Identity.Domain.Common.Results;

namespace Identity.Application.Features.GeographicLookup.Queries.ListCitiesByGovernorate;

public sealed record ListCitiesByGovernorateQuery(Guid GovernorateId)
    : ICachedQuery<Result<List<CityDto>>>
{
    public string CacheKey => $"lookups:cities:{GovernorateId}";
    public string[] Tags => ["cities", $"governorate:{GovernorateId}"];
    public TimeSpan Expiration => TimeSpan.FromHours(24);
    public TimeSpan? LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
}