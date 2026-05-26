using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.Admin.ResetPlayerUndo;

public sealed class ResetPlayerUndoCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public ResetPlayerUndoCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Undo.PlayerUndoInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
