using LexiLink.Common.Application.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Diamond.Infrastructure.Outbox.DomainEventNotifications;

/// <summary>
/// Diamond-owned audit notification. The module keeps its own payload
/// type so outbox serialization remains local to Diamond admin commands.
/// </summary>
public sealed class DiamondAdminActionPerformedNotification : IDomainEventNotification
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string TargetType { get; private set; } = null!;
    public string? TargetId { get; private set; }
    public string PayloadJson { get; private set; } = null!;

    public DiamondAdminActionPerformedNotification(
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
    private DiamondAdminActionPerformedNotification()
    {
    }
}
