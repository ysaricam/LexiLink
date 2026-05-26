using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.Admin.GrantBonusReset;

/// <summary>
/// Admin variant of the internal GrantResetCommand. The internal
/// command is invoked by Quest reward delivery (Sprint UR Reset
/// consumer); this admin-marked twin gives ops a way to grant bonus
/// resets directly and is audited via the IAdminCommand marker.
/// </summary>
public sealed class GrantBonusResetCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantBonusResetCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Reset.PlayerResetInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
