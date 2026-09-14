using Identity.Application.Common.Interfaces;
using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using Identity.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.RoleManagement.Commands.ChangeUserRole;

public sealed class ChangeUserRoleCommandHandler(
    IIdentityService identityService,
    IAuditLogService auditLogService,
    ILogger<ChangeUserRoleCommandHandler> logger)
    : IRequestHandler<ChangeUserRoleCommand, Result<ChangeUserRoleResponse>>
{
    public async Task<Result<ChangeUserRoleResponse>> Handle(
        ChangeUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Role change requested for user {UserId} -> {NewRole}",
            command.UserId,
            command.Request.NewRole);

        // 1. Apply the role change through the identity provider
        var result = await identityService.ChangeUserRoleAsync(
            command.UserId,
            command.Request.NewRole,
            cancellationToken);

        if (result.IsError)
            return result.Errors;

        var response = result.Value;

        // 2. Record the change in the audit log
        await auditLogService.LogAsync(
            targetUserId: command.UserId,
            action: "RoleChanged",
            oldValue: response.OldRole,
            newValue: response.NewRole,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Role changed successfully for user {UserId}: {OldRole} -> {NewRole}",
            command.UserId,
            response.OldRole,
            response.NewRole);

        return response;
    }
}