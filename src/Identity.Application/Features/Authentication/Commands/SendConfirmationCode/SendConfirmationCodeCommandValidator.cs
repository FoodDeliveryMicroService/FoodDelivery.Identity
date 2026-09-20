using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.SendConfirmationCode
{
    public sealed class SendConfirmationCodeCommandValidator
    : AbstractValidator<SendConfirmationCodeCommand>
    {
        public SendConfirmationCodeCommandValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
