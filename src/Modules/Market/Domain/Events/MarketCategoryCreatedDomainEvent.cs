using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class MarketCategoryCreatedDomainEvent : DomainEvent
{
    public MarketCategoryCreatedDomainEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}
