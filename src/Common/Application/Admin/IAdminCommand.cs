namespace LexiLink.Common.Application.Admin;

/// <summary>
/// Marker interface for application commands that require an authorized
/// admin principal. The per-module
/// <c>AdminAuditingCommandHandlerDecorator</c> discovers admin commands
/// through this marker and emits an audit event with the supplied
/// target metadata.
/// </summary>
public interface IAdminCommand
{
    /// <summary>
    /// Stable string identifying the target aggregate / read model the
    /// command mutates. Convention: "<c>{Module}.{AggregateName}</c>",
    /// e.g. "Quests.QuestDefinition", "Quests.PlayerQuest". Used by
    /// audit filtering — keep it stable across renames.
    /// </summary>
    string AuditTargetType { get; }

    /// <summary>
    /// Stringified identifier of the specific target instance, or null
    /// when the command operates on a target whose id is allocated
    /// inside the handler (e.g. Create commands). For those cases the
    /// PayloadJson on the audit row carries the freshly-allocated id.
    /// </summary>
    string? AuditTargetId { get; }
}
