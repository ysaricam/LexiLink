using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.Admin.GrantBonusHint;

/// <summary>
/// Admin variant of the internal GrantHintCommand. The internal
/// command is invoked by Quest reward delivery (Sprint H Hint
/// consumer); this admin-marked twin gives ops a way to grant bonus
/// hints directly and is audited via the IAdminCommand marker.
/// </summary>
public sealed class GrantBonusHintCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantBonusHintCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Hint.PlayerHintInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
