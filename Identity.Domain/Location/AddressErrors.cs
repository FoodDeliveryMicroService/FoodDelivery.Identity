using Identity.Domain.Common.Results;

namespace Identity.Domain.Location;

public static class AddressErrors
{
    public static readonly Error InvalidCustomer =
        Error.Validation(
            "Address.Customer.Invalid",
            "Address customer is required.");

    public static readonly Error InvalidLabel =
        Error.Validation(
            "Address.Label.Required",
            "Address label is required.");

    public static readonly Error InvalidGovernorate =
        Error.Validation(
            "Address.Governorate.Invalid",
            "Address governorate is required.");

    public static readonly Error InvalidCity =
        Error.Validation(
            "Address.City.Invalid",
            "Address city is required.");

    public static readonly Error InvalidStreet =
        Error.Validation(
            "Address.Street.Required",
            "Address street is required.");

    public static readonly Error InvalidBuildingNumber =
        Error.Validation(
            "Address.BuildingNumber.Required",
            "Address building number is required.");

    public static readonly Error InvalidLatitude =
        Error.Validation(
            "Address.Latitude.Invalid",
            "Latitude must be between -90 and 90.");

    public static readonly Error InvalidLongitude =
        Error.Validation(
            "Address.Longitude.Invalid",
            "Longitude must be between -180 and 180.");

    public static readonly Error AlreadyDefault =
        Error.Conflict(
            "Address.AlreadyDefault",
            "Address is already the default address.");

    public static readonly Error NotDefault =
        Error.Conflict(
            "Address.NotDefault",
            "Address is not the default address.");

    public static readonly Error NotFound =
        Error.NotFound(
            "Address.NotFound",
            "Address was not found.");

    public static readonly Error Unauthorized =
        Error.Unauthorized(
            "Address.Unauthorized",
            "You are not authorized to manage this address.");

    public static readonly Error DuplicateLabel =
        Error.Conflict(
            "Address.Label.Duplicate",
            "An address with this label already exists.");

    public static readonly Error MaximumAddressesReached =
        Error.Conflict(
            "Address.MaximumReached",
            "The maximum number of saved addresses has been reached.");

    public static readonly Error InvalidLocation =
        Error.Validation(
            "Address.Location.Invalid",
            "The selected city does not belong to the selected governorate.");
}