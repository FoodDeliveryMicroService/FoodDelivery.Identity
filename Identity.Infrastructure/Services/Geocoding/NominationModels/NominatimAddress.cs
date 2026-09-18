using System.Text.Json.Serialization;

namespace Identity.Infrastructure.Services.Geocoding.NominationModels
{
    internal sealed record NominatimAddress(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("town")] string? Town,
    [property: JsonPropertyName("village")] string? Village,
    [property: JsonPropertyName("country_code")] string? CountryCode);
}
