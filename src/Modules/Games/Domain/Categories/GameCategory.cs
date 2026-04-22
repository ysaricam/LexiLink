using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Categories.Events;
using LexiLink.Modules.Games.Domain.Categories.Rules;

namespace LexiLink.Modules.Games.Domain.Categories;

public sealed class GameCategory : AggregateRoot
{
    public GameCategoryId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private GameCategory(string name, string description = "")
    {
        CheckRule(new GameCategoryNameCannotBeEmptyRule(name));

        Id = new GameCategoryId(Guid.NewGuid());
        Name = name.Trim();
        Description = description;

        this.AddDomainEvent(new GameCategoryCreatedDomainEvent(this.Id, this.Name));
    }

    internal static GameCategory CreateNew(string name, string description = "")
    {
        return new GameCategory(name, description);
    }
}