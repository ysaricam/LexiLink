using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Energy.Application.Contracts;

namespace LexiLink.Modules.Energy.Application.Admin.ResetPlayerEnergy;

public sealed class ResetPlayerEnergyCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public ResetPlayerEnergyCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Energy.PlayerEnergy";
    public string? AuditTargetId => PlayerId.ToString();
}
