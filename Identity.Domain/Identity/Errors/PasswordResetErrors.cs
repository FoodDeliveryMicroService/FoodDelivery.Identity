using Identity.Domain.Common.Results;

namespace Identity.Domain.Identity.Errors;

public static class PasswordResetErrors
{
    public static readonly Error InvalidOrExpiredToken =
        Error.BadRequest("PasswordReset_InvalidOrExpiredToken", "The reset token is invalid or has expired.");
}