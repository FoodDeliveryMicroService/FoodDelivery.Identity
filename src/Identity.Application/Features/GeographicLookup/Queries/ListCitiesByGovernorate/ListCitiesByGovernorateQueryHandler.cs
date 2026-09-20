using Identity.Application.Common.Interfaces;
using Identity.Application.Features.GeographicLookup.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.GeographicLookup.Queries.ListCitiesByGovernorate;

public sealed class ListCitiesByGovernorateQueryHandler(IAppDbContext context, ILogger<ListCitiesByGovernorateQueryHandler> logger)
    : IRequestHandler<ListCitiesByGovernorateQuery, Result<List<CityDto>>>
{
    public async Task<Result<List<CityDto>>> Handle(
        ListCitiesByGovernorateQuery query,
        CancellationToken cancellationToken)
    {
        var governorateExists = await context.Governorates
            .AnyAsync(g => g.Id == query.GovernorateId, cancellationToken);

        if (!governorateExists)
            return GovernorateErrors.NotFound;

        logger.LogInformation("Fetching all the cities belonging to {GovernorateId}", query.GovernorateId);

        var cities = await context.Cities
            .Where(c => c.GovernorateId == query.GovernorateId)
            .OrderBy(c => c.NameAr)
            .Select(c => new CityDto(c.Id, c.GovernorateId, c.NameAr, c.NameEn))
            .ToListAsync(cancellationToken);

        return cities;
    }
}