using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Categories;

public sealed class GameCategoryId : TypedIdValueBase
{
    public GameCategoryId(Guid value) : base(value)
    {
    }
}