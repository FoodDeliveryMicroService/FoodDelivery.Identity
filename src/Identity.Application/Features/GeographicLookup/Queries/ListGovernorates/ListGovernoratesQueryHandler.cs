using Identity.Application.Common.Interfaces;
using Identity.Application.Features.GeographicLookup.Dtos;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.GeographicLookup.Queries.ListGovernorates;

public sealed class ListGovernoratesQueryHandler(IAppDbContext context, ILogger<ListGovernoratesQueryHandler> logger)
    : IRequestHandler<ListGovernoratesQuery, Result<List<GovernorateDto>>>
{
    public async Task<Result<List<GovernorateDto>>> Handle(
        ListGovernoratesQuery query,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching list of governorates of Egypt");

        var governorates = await context.Governorates
            .OrderBy(g => g.NameAr)
            .Select(g => new GovernorateDto(g.Id, g.NameAr, g.NameEn))
            .ToListAsync(cancellationToken);

        return governorates;
    }
}