using Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;
using Identity.Domain.Common.Results;
using MediatR;

namespace Identity.Application.Features.RoleManagement.Commands.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid UserId, ChangeUserRoleRequest Request)
    : IRequest<Result<ChangeUserRoleResponse>>;