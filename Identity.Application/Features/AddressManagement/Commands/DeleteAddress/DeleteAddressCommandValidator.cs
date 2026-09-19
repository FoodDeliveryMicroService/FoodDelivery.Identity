using FluentValidation;

namespace Identity.Application.Features.AddressManagement.Commands.DeleteAddress;

public sealed class DeleteAddressCommandValidator : AbstractValidator<DeleteAddressCommand>
{
    public DeleteAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}