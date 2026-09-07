using Identity.Domain.Common;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public sealed class City : AuditableEntity
{
    public Guid GovernorateId { get; private set; }

    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private City()
    {
    }

    private City(
        Guid id,
        Guid governorateId,
        string nameAr,
        string nameEn)
        : base(id)
    {
        GovernorateId = governorateId;
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public static Result<City> Create(
        Guid id,
        Guid governorateId,
        string nameAr,
        string nameEn)
    {
        if (id == Guid.Empty)
            return CityErrors.InvalidId;

        if (governorateId == Guid.Empty)
            return CityErrors.InvalidGovernorate;

        if (string.IsNullOrWhiteSpace(nameAr))
            return CityErrors.InvalidArabicName;

        if (string.IsNullOrWhiteSpace(nameEn))
            return CityErrors.InvalidEnglishName;

        var city = new City(
            id,
            governorateId,
            nameAr.Trim(),
            nameEn.Trim());

        return city;
    }

    public Result<Updated> UpdateNames(
        string nameAr,
        string nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            return CityErrors.InvalidArabicName;

        if (string.IsNullOrWhiteSpace(nameEn))
            return CityErrors.InvalidEnglishName;

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();

        return Result.Updated;
    }

    public Result<Updated> ChangeGovernorate(Guid governorateId)
    {
        if (governorateId == Guid.Empty)
            return CityErrors.InvalidGovernorate;

        GovernorateId = governorateId;

        return Result.Updated;
    }
}