using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Diamond.Application.Contracts;

namespace LexiLink.Modules.Diamond.Application.Admin.SetPlayerDiamond;

public sealed class SetPlayerDiamondCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Balance { get; }

    public SetPlayerDiamondCommand(Guid playerId, int balance)
    {
        PlayerId = playerId;
        Balance = balance;
    }

    public string AuditTargetType => "Diamond.PlayerDiamondInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
