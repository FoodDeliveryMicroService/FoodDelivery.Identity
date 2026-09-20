using Identity.Application.Common.Interfaces;
using Identity.Application.Features.Profile.Dtos.GetProfile;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Profile.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserCommandHandler(
    IIdentityService identityService,
    IAuditLogService auditLogService,
    ILogger<AdminUpdateUserCommandHandler> logger)
    : IRequestHandler<AdminUpdateUserCommand, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(
        AdminUpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Admin update requested for user {TargetUserId}", command.TargetUserId);

        var result = await identityService.UpdateProfileAsync(
            command.TargetUserId,
            command.Request.Name,
            command.Request.PhoneNumber,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        await auditLogService.LogAsync(
            targetUserId: command.TargetUserId,
            action: "ProfileUpdatedByAdmin",
            newValue: $"Name={result.Value.Name}; Phone={result.Value.PhoneNumber}",
            cancellationToken: cancellationToken);

        return result.Value;
    }
}