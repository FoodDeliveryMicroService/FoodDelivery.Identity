namespace Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;

public sealed record ChangeUserRoleResponse(Guid UserId, string OldRole, string NewRole);