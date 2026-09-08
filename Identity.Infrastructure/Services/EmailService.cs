using FluentEmail.Core;
using Identity.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Identity.Infrastructure.Services;

public sealed class EmailService(
    IFluentEmail fluentEmail,
    ILogger<EmailService> logger,
    IWebHostEnvironment webHostEnvironment)
    : IEmailService
{
    private readonly IFluentEmail _fluentEmail = fluentEmail;
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    public async Task<bool> SendConfirmationCodeAsync(
        string toEmail,
        string userName,
        string confirmationCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Sending confirmation code email to: {Email}",
                toEmail);

            var html = await BuildEmailHtmlAsync(
                userName,
                confirmationCode,
                cancellationToken);

            var email = _fluentEmail
                .To(toEmail, userName)
                .Subject("Confirm Your Email - Wasal")
                .Body(html, isHtml: true);

            var response = await email.SendAsync(cancellationToken);

            if (!response.Successful)
            {
                logger.LogError(
                    "Failed to send confirmation email to {Email}. Errors: {Errors}",
                    toEmail,
                    string.Join(", ", response.ErrorMessages));

                return false;
            }

            logger.LogInformation(
                "Confirmation email sent successfully to: {Email}",
                toEmail);

            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Sending confirmation email was cancelled for: {Email}",
                toEmail);

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while sending confirmation email to: {Email}",
                toEmail);

            return false;
        }
    }

    private async Task<string> BuildEmailHtmlAsync(
        string userName,
        string confirmationCode,
        CancellationToken cancellationToken)
    {
        var templatesPath = Path.Combine(
            _webHostEnvironment.WebRootPath,
            "email-templates");

        var htmlPath = Path.Combine(
            templatesPath,
            "confirmation-code.html");

        var cssPath = Path.Combine(
            templatesPath,
            "confirmation-code.css");

        if (!File.Exists(htmlPath))
        {
            throw new FileNotFoundException(
                "Confirmation email HTML template was not found.",
                htmlPath);
        }

        if (!File.Exists(cssPath))
        {
            throw new FileNotFoundException(
                "Confirmation email CSS file was not found.",
                cssPath);
        }

        var html = await File.ReadAllTextAsync(
            htmlPath,
            Encoding.UTF8,
            cancellationToken);

        var css = await File.ReadAllTextAsync(
            cssPath,
            Encoding.UTF8,
            cancellationToken);

        html = html
            .Replace("{{Styles}}", css)
            .Replace(
                "{{UserName}}",
                System.Net.WebUtility.HtmlEncode(userName))
            .Replace(
                "{{ConfirmationCode}}",
                System.Net.WebUtility.HtmlEncode(confirmationCode));

        return html;
    }
}