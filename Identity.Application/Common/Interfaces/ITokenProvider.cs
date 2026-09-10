using System.Security.Claims;
using Identity.Application.Features.Identity;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;

namespace Identity.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(
        AppUserDto user,
        CancellationToken cancellationToken = default);

    Task<Result<TokenResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result<Success>> RevokeTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(
        string token);
}