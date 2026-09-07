using Identity.Domain.Common;
using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public sealed class Governorate : AuditableEntity
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private Governorate()
    {
    }

    private Governorate(
        Guid id,
        string nameAr,
        string nameEn)
        : base(id)
    {
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public static Result<Governorate> Create(
        Guid id,
        string nameAr,
        string nameEn)
    {
        if (id == Guid.Empty)
            return GovernorateErrors.InvalidId;

        if (string.IsNullOrWhiteSpace(nameAr))
            return GovernorateErrors.InvalidArabicName;

        if (string.IsNullOrWhiteSpace(nameEn))
            return GovernorateErrors.InvalidEnglishName;

        var governorate = new Governorate(
            id,
            nameAr.Trim(),
            nameEn.Trim());

        return governorate;
    }

    public Result<Updated> UpdateNames(
        string nameAr,
        string nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            return GovernorateErrors.InvalidArabicName;

        if (string.IsNullOrWhiteSpace(nameEn))
            return GovernorateErrors.InvalidEnglishName;

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();

        return Result.Updated;
    }
}