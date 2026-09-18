using System.Net.Http.Json;
using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;
using Identity.Infrastructure.Services.Geocoding.NominationModels;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Services.Geocoding;

public sealed class GeocodingService(
    HttpClient httpClient,
    IOptions<GeocodingSettings> settingsOptions,
    ILogger<GeocodingService> logger)
    : IGeocodingService
{
    private readonly GeocodingSettings _settings = settingsOptions.Value;

    public async Task<Result<GeocodedAddressDto>> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{_settings.BaseUrl}?lat={latitude}&lon={longitude}" +
            "&format=jsonv2&addressdetails=1&accept-language=ar";

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Nominatim returned {StatusCode} for ({Lat}, {Lon})",
                    response.StatusCode, latitude, longitude);

                return LocationResolutionErrors.ProviderUnavailable;
            }

            var payload = await response.Content
                .ReadFromJsonAsync<NominatimReverseResponse>(cancellationToken: cancellationToken);

            if (payload is null || payload.Error is not null || payload.Address is null)
                return LocationResolutionErrors.UnresolvedLocation;

            var cityName = payload.Address.City ?? payload.Address.Town ?? payload.Address.Village;

            return new GeocodedAddressDto(
                latitude,
                longitude,
                payload.Address.State,
                cityName,
                payload.DisplayName);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error calling Nominatim for ({Lat}, {Lon})",
                latitude, longitude);

            return LocationResolutionErrors.ProviderUnavailable;
        }
    }
}