using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;

public class RemoveOutgoingLinkCommand : CommandBase, IAdminCommand
{
    public Guid LinkId { get; }
    public Guid OutgoingLinkId { get; }

    public RemoveOutgoingLinkCommand(Guid linkId, Guid outgoingLinkId)
    {
        LinkId = linkId;
        OutgoingLinkId = outgoingLinkId;
    }

    public string AuditTargetType => "Games.Link";
    public string? AuditTargetId => LinkId.ToString();
}