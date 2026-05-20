using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Admin.BanPlayer;

public sealed class BanPlayerCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }
    public string Reason { get; }

    public BanPlayerCommand(Guid playerId, string reason)
    {
        PlayerId = playerId;
        Reason = reason;
    }

    public string AuditTargetType => "Players.Player";
    public string? AuditTargetId => PlayerId.ToString();
}
