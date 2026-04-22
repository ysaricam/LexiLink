using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Domain.Games.Events;

public class GameCreatedDomainEvent : DomainEventBase
{
    public GameId GameId { get; }
    public GameCategoryId CategoryId { get; }

    public GameCreatedDomainEvent(GameId gameId, GameCategoryId categoryId)
    {
        GameId = gameId;
        CategoryId = categoryId;
    }
}