using System.Text.Json.Serialization;

namespace Identity.Domain.Identity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        Customer = 1,
        Admin = 2,
        RestaurantOwner = 3,
    }
}
