using FluentValidation;

namespace Identity.Application.Features.RoleManagement.Commands.ChangeUserRole;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Request.NewRole)
            .IsInEnum()
            .WithMessage("The specified role is not valid.");
    }
}