using Identity.Domain.Common.Results;
using Identity.Domain.Identity.Entities;

namespace Identity.Tests.Common.Identity;

public static class RefreshTokenFactory
{
    public static Result<RefreshToken> CreateRefreshToken(
        Guid? id = null,
        string? token = null,
        string? userId = null,
        DateTimeOffset? expiresOnUtc = null)
    {
        return RefreshToken.Create(
            id ?? Guid.NewGuid(),
            token ?? "some-refresh-token",
            userId ?? Guid.NewGuid().ToString(),
            expiresOnUtc ?? DateTimeOffset.UtcNow.AddDays(7));
    }
}
