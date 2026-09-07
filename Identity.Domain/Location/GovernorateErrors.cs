using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public static class GovernorateErrors
{
    public static readonly Error InvalidId =
        Error.Validation(
            "Governorate.Id.Invalid",
            "Governorate ID is required.");

    public static readonly Error InvalidArabicName =
        Error.Validation(
            "Governorate.NameAr.Required",
            "Governorate Arabic name is required.");

    public static readonly Error InvalidEnglishName =
        Error.Validation(
            "Governorate.NameEn.Required",
            "Governorate English name is required.");

    public static readonly Error NotFound =
        Error.NotFound(
            "Governorate.NotFound",
            "Governorate was not found.");

    public static readonly Error DuplicateArabicName =
        Error.Conflict(
            "Governorate.NameAr.Duplicate",
            "A governorate with this Arabic name already exists.");

    public static readonly Error DuplicateEnglishName =
        Error.Conflict(
            "Governorate.NameEn.Duplicate",
            "A governorate with this English name already exists.");
}