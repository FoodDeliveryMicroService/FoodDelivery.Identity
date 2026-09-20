using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Profile.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IIdentityService identityService,
    ILogger<UpdateProfileCommandHandler> logger)
    : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Profile update requested by user {UserId}", command.UserId);

        return await identityService.UpdateProfileAsync(
            command.UserId,
            command.Request.Name,
            command.Request.PhoneNumber,
            cancellationToken);
    }
}