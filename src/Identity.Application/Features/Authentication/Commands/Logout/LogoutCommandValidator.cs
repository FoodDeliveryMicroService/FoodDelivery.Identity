using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Request.RefreshToken)
            .NotEmpty();
    }
}