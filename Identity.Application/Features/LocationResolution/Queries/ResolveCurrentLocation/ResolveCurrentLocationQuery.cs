using Identity.Application.Features.LocationResolution.Dtos.ResolveLocation;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.LocationResolution.Queries.ResolveCurrentLocation;

public sealed record ResolveCurrentLocationQuery(ResolveLocationRequest Request)
    : IRequest<Result<ResolveLocationResponse>>;