using Identity.Domain.Common.Results;
using Identity.Domain.Location;

namespace Identity.Tests.Common.Location;

public static class CityFactory
{
    public static Result<City> CreateCity(
        Guid? id = null,
        Guid? governorateId = null,
        string? nameAr = null,
        string? nameEn = null)
    {
        return City.Create(
            id ?? Guid.NewGuid(),
            governorateId ?? Guid.NewGuid(),
            nameAr ?? "مدينة نصر",
            nameEn ?? "Nasr City");
    }
}
