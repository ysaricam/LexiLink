using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.GetGameById;

public class GetGameByIdQuery : QueryBase<GameDetailsDto>
{
    public Guid GameId { get; }

    public GetGameByIdQuery(Guid gameId)
    {
        GameId = gameId;
    }
}
