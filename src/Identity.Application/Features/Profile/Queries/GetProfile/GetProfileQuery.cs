using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Profile.Queries.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IRequest<Result<ProfileDto>>;