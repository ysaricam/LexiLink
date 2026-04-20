using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Categories;

public sealed class GameCategory : Entity, IAggregateRoot
{
    public GameCategoryId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private GameCategory(string name, string description = "")
    {
        Id = new GameCategoryId(Guid.NewGuid());
        Name = name;
        Description = description;
    }

    public static GameCategory Create(string name, string description = "")
    {
        return new GameCategory(name, description);
    }
}