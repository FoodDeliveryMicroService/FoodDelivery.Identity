using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Identity;
using Identity.Application.Features.Identity.Dtos;
using Identity.Domain.Common.Results;
using Identity.Domain.Identity;
using Identity.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services
{
    public class TokenProviderService(
        IOptions<JwtSettings> jwtOptions,
        IAppDbContext context) : ITokenProvider
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;
        private readonly IAppDbContext _context = context;

        public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
        {
            var tokenResult = await CreateAsync(user, ct);

            if (tokenResult.IsError)
            {
                return tokenResult.Errors;
            }

            return tokenResult.Value;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudiences = _jwtSettings.Audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }

        private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken ct = default)
        {
            var issuer = _jwtSettings.Issuer;
            var audiences = _jwtSettings.Audience;
            var key = _jwtSettings.Secret;
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
            };

            foreach (var audience in audiences)
            {
                descriptor.Audiences.Add(audience);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(descriptor);

            var oldRefreshTokens = await _context.RefreshTokens
                  .Where(rt => rt.UserId == user.UserId.ToString())
                  .ExecuteDeleteAsync(ct);

            var refreshTokenResult = RefreshToken.Create(
                Guid.NewGuid(),
                GenerateRefreshToken(),
                user.UserId.ToString(),
                DateTime.UtcNow.AddDays(7));

            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Errors;
            }

            var refreshToken = refreshTokenResult.Value;

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(ct);

            return new TokenResponse
            {
                AccessToken = tokenHandler.WriteToken(securityToken),
                RefreshToken = refreshToken.Token,
                ExpiresOnUtc = expires
            };
        }

        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}