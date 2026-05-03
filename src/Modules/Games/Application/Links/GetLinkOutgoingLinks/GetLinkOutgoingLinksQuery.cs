using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;

public class GetLinkOutgoingLinksQuery : QueryBase<List<OutgoingLinkDto>>
{
    public Guid LinkId { get; }

    public GetLinkOutgoingLinksQuery(Guid linkId)
    {
        LinkId = linkId;
    }
}
