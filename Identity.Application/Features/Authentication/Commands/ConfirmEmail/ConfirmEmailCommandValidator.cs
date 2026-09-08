using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator
    : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .Length(6);
    }
}