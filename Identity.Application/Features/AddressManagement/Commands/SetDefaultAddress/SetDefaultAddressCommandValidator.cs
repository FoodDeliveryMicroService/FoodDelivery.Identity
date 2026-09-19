using FluentValidation;

namespace Identity.Application.Features.AddressManagement.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressCommandValidator : AbstractValidator<SetDefaultAddressCommand>
{
    public SetDefaultAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}