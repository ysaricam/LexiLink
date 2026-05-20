using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Admin.UnbanPlayer;

public sealed class UnbanPlayerCommand : CommandBase, IAdminCommand
{
    public Guid PlayerId { get; }

    public UnbanPlayerCommand(Guid playerId)
    {
        PlayerId = playerId;
    }

    public string AuditTargetType => "Players.Player";
    public string? AuditTargetId => PlayerId.ToString();
}
