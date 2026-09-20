using FluentValidation;

namespace Identity.Application.Features.AddressManagement.Queries.GetAddress;

public sealed class GetAddressQueryValidator : AbstractValidator<GetAddressQuery>
{
    public GetAddressQueryValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}