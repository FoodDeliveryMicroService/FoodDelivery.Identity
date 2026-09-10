using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Identity;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Identity.Entities;
using Identity.Domain.Identity.Errors;
using Identity.Infrastructure.Identity;
using Identity.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

public sealed class TokenProviderService(
    IOptions<JwtSettings> jwtOptions,
    IAppDbContext context,
    UserManager<AppUser> userManager)
    : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;
    private readonly IAppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;

    // ============================================================
    // 1. Generate JWT + Refresh Token
    // ============================================================

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(
    AppUserDto user,
    CancellationToken cancellationToken = default)
    {
        var tokenResult = await CreateTokenPairAsync(
            user,
            cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        await _context.SaveChangesAsync(cancellationToken);

        return tokenResult.Value;
    }

    // ============================================================
    // 2. Refresh Token
    // ============================================================

    public async Task<Result<TokenResponse>> RefreshTokenAsync(
     string refreshToken,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthenticationErrors.InvalidRefreshToken;

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.Token == refreshToken,
                cancellationToken);

        if (storedToken is null)
            return AuthenticationErrors.InvalidRefreshToken;

        if (storedToken.IsRevoked)
            return AuthenticationErrors.RefreshTokenRevoked;

        if (storedToken.ExpiresOnUtc <= DateTimeOffset.UtcNow)
            return AuthenticationErrors.RefreshTokenExpired;

        var user = await _userManager.FindByIdAsync(
            storedToken.UserId);

        if (user is null)
            return AuthenticationErrors.InvalidRefreshToken;

        var roles = await _userManager.GetRolesAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);

        var userDto = new AppUserDto(
            user.Id,
            user.Email!,
            roles,
            claims);

        // Revoke old token
        var revokeResult = storedToken.Revoke();

        if (revokeResult.IsError)
            return revokeResult.Errors;

        // Create new pair
        var tokenResult = await CreateTokenPairAsync(
            userDto,
            cancellationToken);

        if (tokenResult.IsError)
            return tokenResult.Errors;

        // Save BOTH:
        // 1. old token as revoked
        // 2. new refresh token
        await _context.SaveChangesAsync(cancellationToken);

        return tokenResult.Value;
    }

    // ============================================================
    // 3. Revoke Refresh Token
    // ============================================================

    public async Task<Result<Success>> RevokeTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return AuthenticationErrors.InvalidRefreshToken;

        // --------------------------------------------------------
        // 1. Find token
        // --------------------------------------------------------

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.Token == refreshToken,
                cancellationToken);

        if (storedToken is null)
            return AuthenticationErrors.InvalidRefreshToken;

        // --------------------------------------------------------
        // 2. Check if already revoked
        // --------------------------------------------------------

        if (storedToken.IsRevoked)
            return AuthenticationErrors.RefreshTokenRevoked;

        // --------------------------------------------------------
        // 3. Revoke
        // --------------------------------------------------------

        var revokeResult = storedToken.Revoke();

        if (revokeResult.IsError)
            return revokeResult.Errors;

        // --------------------------------------------------------
        // 4. Save
        // --------------------------------------------------------

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    // ============================================================
    // 4. Get Principal From Expired Access Token
    // ============================================================

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(
        string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _jwtSettings.Secret)),

            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudiences = _jwtSettings.Audience,

            // This method is specifically used for
            // expired access tokens.
            ValidateLifetime = false,

            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken)
                return null;

            if (!jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    // ============================================================
    // 5. Create Access + Refresh Token Pair
    // ============================================================

    private async Task<Result<TokenResponse>> CreateTokenPairAsync(
    AppUserDto user,
    CancellationToken cancellationToken = default)
    {
        var accessTokenExpiresOnUtc =
            DateTimeOffset.UtcNow.AddMinutes(
                _jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
    {
        new(
            JwtRegisteredClaimNames.Sub,
            user.UserId.ToString()),

        new(
            JwtRegisteredClaimNames.Email,
            user.Email)
    };

        foreach (var role in user.Roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _jwtSettings.Secret));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256Signature);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = accessTokenExpiresOnUtc.UtcDateTime,

            Issuer = _jwtSettings.Issuer,

            SigningCredentials = signingCredentials
        };

        foreach (var audience in _jwtSettings.Audience)
        {
            descriptor.Audiences.Add(audience);
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(
            descriptor);

        var accessToken = tokenHandler.WriteToken(
            securityToken);

        var refreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId.ToString(),
            DateTimeOffset.UtcNow.AddDays(7));

        if (refreshTokenResult.IsError)
            return refreshTokenResult.Errors;

        var refreshToken = refreshTokenResult.Value;

        _context.RefreshTokens.Add(refreshToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresOnUtc = accessTokenExpiresOnUtc
        };
    }

    // ============================================================
    // 6. Generate Cryptographically Secure Refresh Token
    // ============================================================

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }
}