using Identity.Domain.Common.Results;

namespace Identity.Domain.Identity.Errors;

public static class RoleErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("Role_UserNotFound", "User not found.");

    public static readonly Error SameRole =
        Error.Conflict("Role_SameRole", "User already has this role.");

    public static readonly Error AssignmentFailed =
        Error.Failure("Role_AssignmentFailed", "Failed to assign the new role to the user.");

    public static readonly Error RemovalFailed =
        Error.Failure("Role_RemovalFailed", "Failed to remove the user's current role.");
}