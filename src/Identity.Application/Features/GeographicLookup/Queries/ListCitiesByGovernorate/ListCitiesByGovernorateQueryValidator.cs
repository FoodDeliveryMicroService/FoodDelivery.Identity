using FluentValidation;

namespace Identity.Application.Features.GeographicLookup.Queries.ListCitiesByGovernorate;

public sealed class ListCitiesByGovernorateQueryValidator : AbstractValidator<ListCitiesByGovernorateQuery>
{
    public ListCitiesByGovernorateQueryValidator()
    {
        RuleFor(x => x.GovernorateId).NotEmpty();
    }
}