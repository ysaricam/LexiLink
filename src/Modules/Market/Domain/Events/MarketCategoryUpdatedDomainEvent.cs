using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class MarketCategoryUpdatedDomainEvent : DomainEvent
{
    public MarketCategoryUpdatedDomainEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}
