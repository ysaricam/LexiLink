using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Events;

public class CategoryCreatedDomainEvent : DomainEvent
{
    public CategoryId CategoryId { get; }

    public CategoryCreatedDomainEvent(CategoryId categoryId)
    {
        CategoryId = categoryId;
    }
}
