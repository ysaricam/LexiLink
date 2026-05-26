using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.Admin.SetPlayerReset;

public sealed class SetPlayerResetCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Balance { get; }

    public SetPlayerResetCommand(Guid playerId, int balance)
    {
        PlayerId = playerId;
        Balance = balance;
    }

    public string AuditTargetType => "Reset.PlayerResetInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
