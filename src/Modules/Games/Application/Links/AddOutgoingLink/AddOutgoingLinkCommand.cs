using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.AddOutgoingLink;

public class AddOutgoingLinkCommand : CommandBase
{
    public Guid LinkId { get; }
    public Guid OutgoingLinkId { get; }

    public AddOutgoingLinkCommand(Guid linkId, Guid outgoingLinkId)
    {
        LinkId = linkId;
        OutgoingLinkId = outgoingLinkId;
    }
}