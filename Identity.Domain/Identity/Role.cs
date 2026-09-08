using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
