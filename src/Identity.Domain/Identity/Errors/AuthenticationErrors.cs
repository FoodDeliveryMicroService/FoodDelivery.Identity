using Identity.Domain.Common.Results;

namespace Identity.Domain.Identity.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized(
            "Auth_InvalidCredentials",
            "Invalid email or password.");

    public static readonly Error AccountSuspended =
        Error.Forbidden(
            "Auth_AccountSuspended",
            "Your account has been suspended. Please contact support.");

    public static readonly Error AccountNotConfirmed =
        Error.Forbidden(
            "Auth_AccountNotConfirmed",
            "Please confirm your email before logging in.");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized(
            "Auth_InvalidRefreshToken",
            "Invalid refresh token.");

    public static readonly Error RefreshTokenRevoked =
        Error.Unauthorized(
            "Auth_RefreshTokenRevoked",
            "Refresh token has been revoked.");

    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized(
            "Auth_RefreshTokenExpired",
            "Refresh token has expired.");
}