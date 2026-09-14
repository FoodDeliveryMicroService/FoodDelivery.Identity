using Identity.Application.Features.Profile.Dtos.AdminUpdateUser;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.Profile.Commands.AdminUpdateUser;

public sealed record AdminUpdateUserCommand(Guid TargetUserId, AdminUpdateUserRequest Request)
    : IRequest<Result<ProfileDto>>;