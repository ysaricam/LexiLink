using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Administration.IntegrationEvents;

/// <summary>
/// Raised by each consumer module's admin auditing decorator after an
/// admin command commits. Administration projects this event into
/// <c>administration.AdminActionAudit</c>. The contract intentionally
/// keeps the payload as a JSON blob — every module's commands have a
/// different shape and central audit storage shouldn't encode that
/// variance.
/// </summary>
/// <param name="Id">Idempotency key for the projection (event id).</param>
/// <param name="OccurredOn">UTC timestamp of the originating command commit.</param>
/// <param name="AdminUserId">Actor — admin who performed the action.</param>
/// <param name="ActionType">Fully-qualified or short command type name.</param>
/// <param name="TargetType">Target aggregate / read model name (e.g. "Energy.PlayerEnergy").</param>
/// <param name="TargetId">Stringified target identifier (uuid or composite). Null when not applicable.</param>
/// <param name="PayloadJson">Serialized command (or before/after snapshot).</param>
public sealed record AdminActionPerformedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AdminUserId,
    string ActionType,
    string TargetType,
    string? TargetId,
    string PayloadJson) : IIntegrationEvent;
