using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class MarketCategoryDeactivatedDomainEvent : DomainEvent
{
    public MarketCategoryDeactivatedDomainEvent(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public Guid CategoryId { get; }
}
