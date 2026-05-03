using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories.Events;
using LexiLink.Modules.Games.Domain.Categories.Rules;

namespace LexiLink.Modules.Games.Domain.Categories;

public class Category : Entity, IAggregateRoot
{
    public CategoryId Id { get; private set; }

    private string _name;
    private string _description;

    // EF Core
    private Category() { }

    private Category(string name, string description)
    {
        CheckRule(new CategoryNameMustNotBeEmptyRule(name));
        CheckRule(new CategoryNameMustNotExceedMaxLengthRule(name));
        CheckRule(new CategoryDescriptionMustNotExceedMaxLengthRule(description));

        Id = new CategoryId(Guid.NewGuid());
        _name = name;
        _description = description;

        AddDomainEvent(new CategoryCreatedDomainEvent(Id));
    }

    public void EditGeneralInfo(string name, string description)
    {
        CheckRule(new CategoryNameMustNotBeEmptyRule(name));
        CheckRule(new CategoryNameMustNotExceedMaxLengthRule(name));
        CheckRule(new CategoryDescriptionMustNotExceedMaxLengthRule(description));

        _name = name;
        _description = description;

        AddDomainEvent(new CategoryEditedDomainEvent(Id));
    }

    internal static Category Create(string name, string description)
    {
        return new Category(name, description);
    }
}