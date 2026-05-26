using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Undo.Application.Contracts;

namespace LexiLink.Modules.Undo.Application.Admin.GrantBonusUndo;

/// <summary>
/// Admin variant of the internal GrantUndoCommand. The internal
/// command is invoked by Quest reward delivery (Sprint UR Undo
/// consumer); this admin-marked twin gives ops a way to grant bonus
/// undos directly and is audited via the IAdminCommand marker.
/// </summary>
public sealed class GrantBonusUndoCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public int Amount { get; }

    public GrantBonusUndoCommand(Guid playerId, int amount)
    {
        PlayerId = playerId;
        Amount = amount;
    }

    public string AuditTargetType => "Undo.PlayerUndoInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
