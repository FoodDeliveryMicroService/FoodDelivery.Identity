using Identity.Domain.Identity;

namespace Identity.Application.Features.RoleManagement.Dtos.ChangeUserRole;

public sealed record ChangeUserRoleRequest(Role NewRole);