namespace Identity.Application.Common.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        Guid targetUserId,
        string action,
        string? oldValue = null,
        string? newValue = null,
        CancellationToken cancellationToken = default);
}