using Identity.Application.Common.Interfaces;
using Identity.Application.Features.LocationResolution.Dtos.ResolveLocation;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.LocationResolution.Queries.ResolveCurrentLocation;

public sealed class ResolveCurrentLocationQueryHandler(
    IGeocodingService geocodingService,
    IAppDbContext context,
    ILogger<ResolveCurrentLocationQueryHandler> logger)
    : IRequestHandler<ResolveCurrentLocationQuery, Result<ResolveLocationResponse>>
{
    public async Task<Result<ResolveLocationResponse>> Handle(
        ResolveCurrentLocationQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;

        logger.LogInformation(
            "Resolving location for ({Lat}, {Lon})",
            request.Latitude, request.Longitude);

        var geocodeResult = await geocodingService.ReverseGeocodeAsync(
            request.Latitude,
            request.Longitude,
            cancellationToken);

        if (geocodeResult.IsError)
            return geocodeResult.Errors;

        var geocoded = geocodeResult.Value;

        var governorate = await MatchGovernorateAsync(geocoded.GovernorateNameRaw, cancellationToken);

        if (governorate is null)
        {
            logger.LogWarning(
                "Could not match governorate '{Raw}' for ({Lat}, {Lon})",
                geocoded.GovernorateNameRaw, request.Latitude, request.Longitude);

            return LocationResolutionErrors.GovernorateNotMatched;
        }

        var city = await MatchCityAsync(geocoded.CityNameRaw, governorate.Id, cancellationToken);

        return new ResolveLocationResponse(
            request.Latitude,
            request.Longitude,
            governorate.Id,
            governorate.NameAr,
            governorate.NameEn,
            city?.Id,
            city?.NameAr,
            city?.NameEn,
            IsFullyResolved: city is not null);
    }

    private async Task<Domain.Location.Governorate?> MatchGovernorateAsync(
        string? rawName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return null;

        var normalized = Normalize(rawName);

        return await context.Governorates
            .Where(g =>
                EF.Functions.Like(g.NameEn, $"%{normalized}%") ||
                EF.Functions.Like(g.NameAr, $"%{normalized}%"))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Domain.Location.City?> MatchCityAsync(
        string? rawName, Guid governorateId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return null;

        var normalized = Normalize(rawName);

        return await context.Cities
            .Where(c => c.GovernorateId == governorateId &&
                (EF.Functions.Like(c.NameEn, $"%{normalized}%") ||
                 EF.Functions.Like(c.NameAr, $"%{normalized}%")))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string Normalize(string name) =>
        name.Replace("Governorate", "", StringComparison.OrdinalIgnoreCase)
            .Replace("محافظة", "")
            .Trim();
}