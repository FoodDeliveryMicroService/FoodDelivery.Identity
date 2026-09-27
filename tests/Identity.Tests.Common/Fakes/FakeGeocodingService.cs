using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Location;

namespace Identity.Tests.Common.Fakes;
public sealed class FakeGeocodingService : IGeocodingService
{
    public GeocodedAddressDto? NextResult { get; set; }
    public Error? NextError { get; set; }

    public Task<Result<GeocodedAddressDto>> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        if (NextError is not null)
            return Task.FromResult<Result<GeocodedAddressDto>>(NextError);

        if (NextResult is not null)
            return Task.FromResult<Result<GeocodedAddressDto>>(NextResult);

        return Task.FromResult<Result<GeocodedAddressDto>>(LocationResolutionErrors.UnresolvedLocation);
    }
}