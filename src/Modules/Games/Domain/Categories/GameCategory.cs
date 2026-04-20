using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Categories;

public sealed class GameCategory : Entity, IAggregateRoot
{
    public GameCategoryId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private GameCategory(string name, string description = "")
    {
        // Kural eklemek gerekirse buraya eklenebilir.
        // CheckRule(new CategoryNameCannotBeEmptyRule(name));

        Id = new GameCategoryId(Guid.NewGuid());
        Name = name;
        Description = description;
    }

    public static GameCategory Create(string name, string description = "")
    {
        return new GameCategory(name, description);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not GameCategory other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(GameCategory? left, GameCategory? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(GameCategory? left, GameCategory? right)
    {
        return !(left == right);
    }
}