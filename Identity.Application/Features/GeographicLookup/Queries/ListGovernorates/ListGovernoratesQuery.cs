using Identity.Application.Common.Interfaces;
using Identity.Application.Features.GeographicLookup.Dtos;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.GeographicLookup.Queries.ListGovernorates;

public sealed record ListGovernoratesQuery : ICachedQuery<Result<List<GovernorateDto>>>
{
    public string CacheKey => "lookups:governorates";
    public string[] Tags => ["governorates"];
    public TimeSpan Expiration => TimeSpan.FromHours(24);
    public TimeSpan? LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
}