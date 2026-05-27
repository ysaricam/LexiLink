using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.Admin.GrantBonusDiamond;

/// <summary>
/// Admin variant of the internal GrantDiamondCommand. Quest reward
/// delivery uses the internal command; this audited command gives ops a
/// direct grant path.
/// </summary>
public sealed class GrantBonusDiamondCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantBonusDiamondCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Diamond.PlayerDiamondInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
