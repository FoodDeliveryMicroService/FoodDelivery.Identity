using System.Text.Json.Serialization;

namespace Identity.Infrastructure.Services.Geocoding.NominationModels
{
    internal sealed record NominatimReverseResponse(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("address")] NominatimAddress? Address,
    [property: JsonPropertyName("error")] string? Error);
}
