using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Application.Features.Profile.Dtos.UpdateProfile;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Profile.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, UpdateProfileRequest Request)
    : IRequest<Result<ProfileDto>>;