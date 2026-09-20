using Identity.Domain.Common.Results;

namespace Identity.Domain.Identity.Errors;

public static class ProfileErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("Profile_UserNotFound", "User not found.");
}