using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Reset.Application.Contracts;

namespace LexiLink.Modules.Reset.Application.Admin.ResetPlayerReset;

/// <summary>
/// Admin snap-to-zero for the player's Reset inventory. The double
/// "Reset" naming is intentional — the *outer* "Reset" matches the
/// Set/Grant/Reset admin verb triplet across all inventory modules
/// (Energy, Hint, Undo, Reset). The *inner* "PlayerReset" is the
/// inventory module name.
/// </summary>
public sealed class ResetPlayerResetCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public ResetPlayerResetCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Reset.PlayerResetInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
