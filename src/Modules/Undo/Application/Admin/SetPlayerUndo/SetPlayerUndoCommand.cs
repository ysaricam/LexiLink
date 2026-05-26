using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.Admin.SetPlayerUndo;

public sealed class SetPlayerUndoCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Balance { get; }

    public SetPlayerUndoCommand(Guid playerId, int balance)
    {
        PlayerId = playerId;
        Balance = balance;
    }

    public string AuditTargetType => "Undo.PlayerUndoInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
