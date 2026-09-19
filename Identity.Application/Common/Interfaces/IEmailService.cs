namespace Identity.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationCodeAsync(
            string toEmail,
            string userName,
            string confirmationCode,
            CancellationToken cancellationToken = default);

        Task<bool> SendPasswordResetEmailAsync(
            string toEmail,
            string userName,
            string resetToken,
            CancellationToken cancellationToken = default);
    }
}
