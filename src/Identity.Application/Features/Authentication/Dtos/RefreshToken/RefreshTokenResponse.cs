namespace Identity.Application.Features.Authentication.Dtos.RefreshToken;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);