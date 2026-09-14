using FluentValidation;

namespace Identity.Application.Features.Profile.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserCommandValidator : AbstractValidator<AdminUpdateUserCommand>
{
    public AdminUpdateUserCommandValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();

        RuleFor(x => x.Request.Name)
            .MaximumLength(100)
            .When(x => x.Request.Name is not null);

        RuleFor(x => x.Request.PhoneNumber)
            .MaximumLength(20)
            .When(x => x.Request.PhoneNumber is not null);
    }
}