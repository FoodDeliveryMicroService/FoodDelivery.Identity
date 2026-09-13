namespace Identity.Application.Features.Authentication.Dtos.Email;

public sealed record EmailConfirmationDto(
    Guid Id,
    string Code,
    DateTimeOffset ExpiresAt
);