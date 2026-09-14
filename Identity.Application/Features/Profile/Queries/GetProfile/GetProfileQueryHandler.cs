using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Profile.Queries.GetProfile;

public sealed class GetProfileQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(
        GetProfileQuery query,
        CancellationToken cancellationToken)
        => await identityService.GetProfileAsync(query.UserId, cancellationToken);
}