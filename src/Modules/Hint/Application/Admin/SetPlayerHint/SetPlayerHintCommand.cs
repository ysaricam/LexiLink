using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.Admin.SetPlayerHint;

public sealed class SetPlayerHintCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Balance { get; }

    public SetPlayerHintCommand(Guid playerId, int balance)
    {
        PlayerId = playerId;
        Balance = balance;
    }

    public string AuditTargetType => "Hint.PlayerHintInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
