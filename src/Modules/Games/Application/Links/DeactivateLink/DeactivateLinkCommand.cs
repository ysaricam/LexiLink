using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.DeactivateLink;

public class DeactivateLinkCommand : CommandBase, IAdminCommand
{
    public Guid LinkId { get; }

    public DeactivateLinkCommand(Guid linkId)
    {
        LinkId = linkId;
    }

    public string AuditTargetType => "Games.Link";
    public string? AuditTargetId => LinkId.ToString();
}
