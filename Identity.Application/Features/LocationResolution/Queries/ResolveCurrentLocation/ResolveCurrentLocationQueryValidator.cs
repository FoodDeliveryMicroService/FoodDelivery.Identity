using FluentValidation;

namespace Identity.Application.Features.LocationResolution.Queries.ResolveCurrentLocation;

public sealed class ResolveCurrentLocationQueryValidator : AbstractValidator<ResolveCurrentLocationQuery>
{
    public ResolveCurrentLocationQueryValidator()
    {
        RuleFor(x => x.Request.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Request.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}