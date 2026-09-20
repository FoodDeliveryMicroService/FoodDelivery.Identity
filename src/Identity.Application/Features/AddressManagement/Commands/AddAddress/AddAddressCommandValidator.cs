using FluentValidation;

namespace Identity.Application.Features.AddressManagement.Commands.AddAddress;

public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Request.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(50);

        RuleFor(x => x.Request.GovernorateId)
            .NotEmpty().WithMessage("Governorate is required.");

        RuleFor(x => x.Request.CityId)
            .NotEmpty().WithMessage("City is required.");

        RuleFor(x => x.Request.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200);

        RuleFor(x => x.Request.BuildingNumber)
            .NotEmpty().WithMessage("Building number is required.")
            .MaximumLength(50);

        RuleFor(x => x.Request.Floor).MaximumLength(20);
        RuleFor(x => x.Request.Apartment).MaximumLength(20);
        RuleFor(x => x.Request.Landmark).MaximumLength(200);

        RuleFor(x => x.Request.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Request.Latitude.HasValue);

        RuleFor(x => x.Request.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Request.Longitude.HasValue);
    }
}