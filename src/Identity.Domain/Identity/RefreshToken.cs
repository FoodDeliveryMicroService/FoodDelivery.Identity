using Identity.Domain.Common;
using Identity.Domain.Common.Results;
using Identity.Domain.Identity.Errors;

namespace Identity.Domain.Identity.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public string Token { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresOnUtc { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedOnUtc { get; private set; }

    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        string token,
        string userId,
        DateTimeOffset expiresOnUtc)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
        IsRevoked = false;
    }

    public static Result<RefreshToken> Create(
        Guid id,
        string token,
        string userId,
        DateTimeOffset expiresOnUtc)
    {
        if (id == Guid.Empty)
            return RefreshTokenErrors.IdRequired;

        if (string.IsNullOrWhiteSpace(token))
            return RefreshTokenErrors.TokenRequired;

        if (string.IsNullOrWhiteSpace(userId))
            return RefreshTokenErrors.UserIdRequired;

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
            return RefreshTokenErrors.ExpiryInvalid;

        return new RefreshToken(
            id,
            token,
            userId,
            expiresOnUtc);
    }

    public Result<Success> Revoke()
    {
        if (IsRevoked)
            return RefreshTokenErrors.AlreadyRevoked;

        IsRevoked = true;
        RevokedOnUtc = DateTimeOffset.UtcNow;

        return Result.Success;
    }
}