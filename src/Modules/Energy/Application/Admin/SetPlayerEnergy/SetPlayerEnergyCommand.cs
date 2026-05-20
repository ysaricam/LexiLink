using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.Admin.SetPlayerEnergy;

public sealed class SetPlayerEnergyCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public SetPlayerEnergyCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Energy.PlayerEnergy";
    public string? AuditTargetId => PlayerId.ToString();
}
