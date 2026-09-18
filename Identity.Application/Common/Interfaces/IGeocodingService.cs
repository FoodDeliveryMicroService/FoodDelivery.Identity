using Identity.Application.Features.AddressManagement.Dtos;
using Identity.Domain.Common.Results;

namespace Identity.Application.Common.Interfaces;

public interface IGeocodingService
{
    Task<Result<GeocodedAddressDto>> ReverseGeocodeAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}