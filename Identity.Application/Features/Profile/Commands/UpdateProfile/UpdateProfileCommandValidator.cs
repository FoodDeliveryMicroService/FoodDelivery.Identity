using FluentValidation;

namespace Identity.Application.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Request.Name)
            .MaximumLength(100)
            .When(x => x.Request.Name is not null);

        RuleFor(x => x.Request.PhoneNumber)
            .MaximumLength(20)
            .When(x => x.Request.PhoneNumber is not null);
    }
}