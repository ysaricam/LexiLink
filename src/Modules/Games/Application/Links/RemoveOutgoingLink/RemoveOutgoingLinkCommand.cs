using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;

public class RemoveOutgoingLinkCommand : CommandBase
{
    public Guid LinkId { get; }
    public Guid OutgoingLinkId { get; }

    public RemoveOutgoingLinkCommand(Guid linkId, Guid outgoingLinkId)
    {
        LinkId = linkId;
        OutgoingLinkId = outgoingLinkId;
    }
}