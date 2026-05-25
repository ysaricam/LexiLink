using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Hint.Application.Contracts;

namespace LexiLink.Modules.Hint.Application.Admin.ResetPlayerHint;

public sealed class ResetPlayerHintCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public ResetPlayerHintCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Hint.PlayerHintInventory";
    public string? AuditTargetId => PlayerId.ToString();
}
