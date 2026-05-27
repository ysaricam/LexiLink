using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.Admin.ResetPlayerDiamond;

public sealed class ResetPlayerDiamondCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public ResetPlayerDiamondCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Diamond.PlayerDiamondInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
