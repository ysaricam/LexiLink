using LexiLink.Common.Application.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Undo.Infrastructure.Outbox.DomainEventNotifications;

/// <summary>
/// Undo-owned audit notification — seventh per-module copy of the
/// Quests B7 template (Kamil decorator-per-module rule). Each module
/// owns its own payload type so the audit serialization tracks the
/// module's exact admin commands.
/// </summary>
public sealed class UndoAdminActionPerformedNotification : IDomainEventNotification
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string TargetType { get; private set; } = null!;
    public string? TargetId { get; private set; }
    public string PayloadJson { get; private set; } = null!;

    public UndoAdminActionPerformedNotification(
        Guid id,
        DateTime occurredOn,
        Guid adminUserId,
        string actionType,
        string targetType,
        string? targetId,
        string payloadJson)
    {
        Id = id;
        OccurredOn = occurredOn;
        AdminUserId = adminUserId;
        ActionType = actionType;
        TargetType = targetType;
        TargetId = targetId;
        PayloadJson = payloadJson;
    }

    [JsonConstructor]
    private UndoAdminActionPerformedNotification()
    {
    }
}
