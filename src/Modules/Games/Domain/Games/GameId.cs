using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games;

public class GameId : TypedIdValueBase
{
    public GameId(Guid value) : base(value)
    {
    }
}