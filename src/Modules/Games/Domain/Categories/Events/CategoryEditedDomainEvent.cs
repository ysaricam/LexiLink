using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Events;

public class CategoryEditedDomainEvent : DomainEvent
{
    public CategoryId CategoryId { get; }

    public CategoryEditedDomainEvent(CategoryId categoryId)
    {
        CategoryId = categoryId;
    }
}
