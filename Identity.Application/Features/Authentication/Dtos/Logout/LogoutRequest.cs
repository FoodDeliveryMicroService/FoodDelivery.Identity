namespace Identity.Application.Features.Authentication.Dtos.Logout;

public sealed record LogoutRequest(
    string RefreshToken
);