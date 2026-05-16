using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;

namespace LexiLink.Modules.Games.Application.Games.GetGameOptions;

public class GetGameOptionsQuery : QueryBase<List<OutgoingLinkDto>>
{
    public Guid GameId { get; }

    public GetGameOptionsQuery(Guid gameId)
    {
        GameId = gameId;
    }
}
