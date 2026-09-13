using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator
    : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Request.userId)
            .NotEmpty();

        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .Length(6);
    }
}