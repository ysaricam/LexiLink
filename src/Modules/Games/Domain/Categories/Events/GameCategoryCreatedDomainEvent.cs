using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Events;

public class GameCategoryCreatedDomainEvent : DomainEventBase
{
    public GameCategoryId CategoryId { get; }
    public string Name { get; }

    public GameCategoryCreatedDomainEvent(GameCategoryId categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}