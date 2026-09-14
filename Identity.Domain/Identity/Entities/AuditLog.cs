using Identity.Domain.Common;

namespace Identity.Domain.Identity.Entities;

public sealed class AuditLog : AuditableEntity
{
    public Guid TargetUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    private AuditLog() { } // For EF Core

    private AuditLog(
        Guid id,
        Guid targetUserId,
        string action,
        string? oldValue,
        string? newValue)
        : base(id)
    {
        TargetUserId = targetUserId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public static AuditLog Create(
        Guid targetUserId,
        string action,
        string? oldValue = null,
        string? newValue = null)
        => new(Guid.NewGuid(), targetUserId, action, oldValue, newValue);
}