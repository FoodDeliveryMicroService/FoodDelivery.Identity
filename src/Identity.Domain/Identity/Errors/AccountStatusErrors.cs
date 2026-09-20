using Identity.Domain.Common.Results;

namespace Identity.Domain.Identity.Errors;

public static class AccountStatusErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("AccountStatus_UserNotFound", "User not found.");

    public static readonly Error AlreadySuspended =
        Error.Conflict("AccountStatus_AlreadySuspended", "This account is already suspended.");

    public static readonly Error AlreadyActive =
        Error.Conflict("AccountStatus_AlreadyActive", "This account is already active.");

    public static readonly Error UpdateFailed =
        Error.Failure("AccountStatus_UpdateFailed", "Failed to update the account status.");
}