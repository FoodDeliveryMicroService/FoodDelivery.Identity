using Identity.Application.Common.Interfaces;
using Identity.Domain.Identity.Entities;

namespace Identity.Infrastructure.Services;

public sealed class AuditLogService(IAppDbContext context) : IAuditLogService
{
    public async Task LogAsync(
        Guid targetUserId,
        string action,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        var log = AuditLog.Create(targetUserId, action, oldValue, newValue);

        await context.AuditLogs.AddAsync(log, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}