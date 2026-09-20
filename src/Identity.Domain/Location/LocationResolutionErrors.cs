using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public static class LocationResolutionErrors
{
    public static readonly Error ProviderUnavailable =
        Error.Failure(
            "LocationResolution.ProviderUnavailable",
            "Could not reach the geocoding provider. Please try again.");

    public static readonly Error UnresolvedLocation =
        Error.NotFound(
            "LocationResolution.Unresolved",
            "The provided coordinates could not be resolved.");

    public static readonly Error GovernorateNotMatched =
        Error.NotFound(
            "LocationResolution.GovernorateNotMatched",
            "The resolved location does not match any supported governorate.");
}