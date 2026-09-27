using Identity.Application.Common.Interfaces;

namespace Identity.Tests.Common.Fakes;

/// <summary>
/// Replaces the real EmailService (SMTP + wwwroot HTML templates) in tests.
/// Captures the last confirmation code / reset token so tests can complete
/// the register -> confirm or forgot -> rebn set flows without a mailbox.
/// </summary>
public sealed class FakeEmailService : IEmailService
{
    public string? LastConfirmationCode { get; private set; }

    public string? LastPasswordResetToken { get; private set; }

    public Task<bool> SendConfirmationCodeAsync(
        string toEmail,
        string userName,
        string confirmationCode,
        CancellationToken cancellationToken = default)
    {
        LastConfirmationCode = confirmationCode;
        return Task.FromResult(true);
    }

    public Task<bool> SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        LastPasswordResetToken = resetToken;
        return Task.FromResult(true);
    }
}
