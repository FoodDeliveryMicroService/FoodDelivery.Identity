using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AccountStatus.Commands.SuspendUser;

public sealed class SuspendUserCommandHandler(
    IIdentityService identityService,
    IAuditLogService auditLogService,
    ILogger<SuspendUserCommandHandler> logger)
    : IRequestHandler<SuspendUserCommand, Result<AccountStatusResponse>>
{
    public async Task<Result<AccountStatusResponse>> Handle(
        SuspendUserCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Suspend requested for user {UserId}", command.UserId);

        var result = await identityService.SuspendUserAsync(command.UserId, cancellationToken);

        if (result.IsError)
            return result.Errors;

        await auditLogService.LogAsync(
            targetUserId: command.UserId,
            action: "AccountSuspended",
            oldValue: "Active",
            newValue: "Suspended",
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} suspended successfully", command.UserId);

        return result.Value;
    }
}