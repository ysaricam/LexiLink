using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.ActivateLink;

public class ActivateLinkCommand : CommandBase, IAdminCommand
{
    public Guid LinkId { get; }

    public ActivateLinkCommand(Guid linkId)
    {
        LinkId = linkId;
    }

    public string AuditTargetType => "Games.Link";
    public string? AuditTargetId => LinkId.ToString();
}
