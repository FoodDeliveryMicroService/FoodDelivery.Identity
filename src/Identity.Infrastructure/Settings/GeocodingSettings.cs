using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Settings
{
    public sealed class GeocodingSettings
    {
        public const string SectionName = "GeocodingSettings";
        public string BaseUrl { get; set; } = "https://nominatim.openstreetmap.org/reverse";
        public string UserAgent { get; set; } = "FoodDeliveryApp/1.0 (contact@example.com)";
    }
}
  