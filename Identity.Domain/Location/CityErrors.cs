using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public static class CityErrors
{
    public static readonly Error InvalidId =
        Error.Validation(
            "City.Id.Invalid",
            "City ID is required.");

    public static readonly Error InvalidGovernorate =
        Error.Validation(
            "City.Governorate.Invalid",
            "City governorate is required.");

    public static readonly Error InvalidArabicName =
        Error.Validation(
            "City.NameAr.Required",
            "City Arabic name is required.");

    public static readonly Error InvalidEnglishName =
        Error.Validation(
            "City.NameEn.Required",
            "City English name is required.");

    public static readonly Error NotFound =
        Error.NotFound(
            "City.NotFound",
            "City was not found.");

    public static readonly Error DuplicateArabicName =
        Error.Conflict(
            "City.NameAr.Duplicate",
            "A city with this Arabic name already exists.");

    public static readonly Error DuplicateEnglishName =
        Error.Conflict(
            "City.NameEn.Duplicate",
            "A city with this English name already exists.");

    public static readonly Error InvalidGovernorateRelationship =
        Error.Validation(
            "City.Governorate.InvalidRelationship",
            "The city does not belong to the specified governorate.");
}