using LexiLink.Common.Application.Events;
using Newtonsoft.Json;

namespace LexiLink.Modules.Games.Infrastructure.Outbox.DomainEventNotifications;

/// <summary>
/// Games-owned audit notification (fourth and final per-module copy of
/// the audit payload type after Quests/Energy/Players). Kamil
/// decorator-per-module rule.
/// </summary>
public sealed class GamesAdminActionPerformedNotification : IDomainEventNotification
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public Guid AdminUserId { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string TargetType { get; private set; } = null!;
    public string? TargetId { get; private set; }
    public string PayloadJson { get; private set; } = null!;

    public GamesAdminActionPerformedNotification(
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
    private GamesAdminActionPerformedNotification()
    {
    }
}
