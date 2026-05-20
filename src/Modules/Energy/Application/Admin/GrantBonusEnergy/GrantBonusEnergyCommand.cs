using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.Admin.GrantBonusEnergy;

/// <summary>
/// Admin variant of the internal GrantEnergyCommand. The internal command
/// is invoked by quest reward delivery; this admin-marked twin gives ops
/// a way to grant bonus energy directly and is audited.
/// </summary>
public sealed class GrantBonusEnergyCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantBonusEnergyCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Energy.PlayerEnergy";
    public string? AuditTargetId => PlayerId.ToString();
}
