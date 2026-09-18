using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Features.AddressManagement.Dtos;

public sealed record GeocodedAddressDto(
    double Latitude,
    double Longitude,
    string? GovernorateNameRaw,
    string? CityNameRaw,
    string? DisplayName);