using Identity.Application.Common.Interfaces;
using Identity.Application.Features.AccountStatus.Dtos;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.AccountStatus.Commands.ReactivateUser;

public sealed class ReactivateUserCommandHandler(
    IIdentityService identityService,
    IAuditLogService auditLogService,
    ILogger<ReactivateUserCommandHandler> logger)
    : IRequestHandler<ReactivateUserCommand, Result<AccountStatusResponse>>
{
    public async Task<Result<AccountStatusResponse>> Handle(
        ReactivateUserCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Reactivate requested for user {UserId}", command.UserId);

        var result = await identityService.ReactivateUserAsync(command.UserId, cancellationToken);

        if (result.IsError)
            return result.Errors;

        await auditLogService.LogAsync(
            targetUserId: command.UserId,
            action: "AccountReactivated",
            oldValue: "Suspended",
            newValue: "Active",
            cancellationToken: cancellationToken);

        logger.LogInformation("User {UserId} reactivated successfully", command.UserId);

        return result.Value;
    }
}