using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories.Events;
using LexiLink.Modules.Games.Domain.Categories.Rules;

namespace LexiLink.Modules.Games.Domain.Categories;

public class Category : Entity, IAggregateRoot
{
    public CategoryId Id { get; private set; }

    private string _name;
    private string _description;
    private string _language;

    public string Language => _language;

    // EF Core
    private Category() { }

    private Category(string name, string description, string language)
    {
        CheckRule(new CategoryNameMustNotBeEmptyRule(name));
        CheckRule(new CategoryNameMustNotExceedMaxLengthRule(name));
        CheckRule(new CategoryDescriptionMustNotExceedMaxLengthRule(description));
        CheckRule(new CategoryLanguageMustBeValidFormatRule(language));

        Id = new CategoryId(Guid.NewGuid());
        _name = name;
        _description = description;
        _language = language;

        AddDomainEvent(new CategoryCreatedDomainEvent(Id));
    }

    public void EditGeneralInfo(string name, string description, string language)
    {
        CheckRule(new CategoryNameMustNotBeEmptyRule(name));
        CheckRule(new CategoryNameMustNotExceedMaxLengthRule(name));
        CheckRule(new CategoryDescriptionMustNotExceedMaxLengthRule(description));
        CheckRule(new CategoryLanguageMustBeValidFormatRule(language));

        _name = name;
        _description = description;
        _language = language;

        AddDomainEvent(new CategoryEditedDomainEvent(Id));
    }

    internal static Category Create(string name, string description, string language = "tr-TR")
    {
        return new Category(name, description, language);
    }
}
