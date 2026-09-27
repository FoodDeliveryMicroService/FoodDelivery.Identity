using Identity.Domain.Email;

namespace Identity.Tests.Common.Identity;

public static class EmailConfirmationFactory
{
    public static EmailConfirmation CreateEmailConfirmation(
        string? userId = null,
        string? code = null,
        DateTimeOffset? expiresAt = null)
    {
        return new EmailConfirmation(
            userId ?? Guid.NewGuid().ToString(),
            code ?? "123456",
            expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(15));
    }
}
