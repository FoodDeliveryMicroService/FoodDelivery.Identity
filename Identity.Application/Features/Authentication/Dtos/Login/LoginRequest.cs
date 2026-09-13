namespace Identity.Application.Features.Authentication.Dtos.Login;

public sealed record LoginRequest(
    string Email,
    string Password
);
