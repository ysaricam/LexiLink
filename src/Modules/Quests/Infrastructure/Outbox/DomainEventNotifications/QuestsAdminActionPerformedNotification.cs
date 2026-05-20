using LexiLink.Common.Application.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Quests.Infrastructure.Outbox.DomainEventNotifications;

/// <summary>
/// Outbox payload that the per-module
/// <c>AdminAuditingCommandHandlerDecorator</c> writes after a Quests
/// admin command commits. The notification handler publishes the
/// public <c>AdminActionPerformedIntegrationEvent</c> via
/// <c>IEventsBus</c> when the outbox processor drains the row;
/// Administration's projection writes the row into AdminActionAudit.
///
/// Lives per-module (Quests-owned copy) on purpose — Kamil's
/// decorator-per-module rule. Each consumer module's audit decorator
/// owns the notification + handler pair for its own outbox.
/// </summary>
public sealed class QuestsAdminActionPerformedNotification : IDomainEventNotification
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string TargetType { get; private set; } = null!;
    public string? TargetId { get; private set; }
    public string PayloadJson { get; private set; } = null!;

    public QuestsAdminActionPerformedNotification(
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
    private QuestsAdminActionPerformedNotification()
    {
    }
}
