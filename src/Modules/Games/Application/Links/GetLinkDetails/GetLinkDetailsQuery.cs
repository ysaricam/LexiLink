using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.GetLinkDetails;

public class GetLinkDetailsQuery : QueryBase<LinkDetailsDto>
{
    public Guid LinkId { get; }
    public GetLinkDetailsQuery(Guid linkId)
    {
        LinkId = linkId;
    }
}