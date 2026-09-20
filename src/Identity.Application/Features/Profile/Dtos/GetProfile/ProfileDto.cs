namespace Identity.Application.Features.Profile.Dtos.GetProfile;

public sealed record ProfileDto(
    Guid UserId,
    string Email,
    string? Name,
    string? PhoneNumber,
    IList<string> Roles);