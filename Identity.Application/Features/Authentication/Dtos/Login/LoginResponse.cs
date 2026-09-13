namespace Identity.Application.Features.Authentication.Dtos.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
