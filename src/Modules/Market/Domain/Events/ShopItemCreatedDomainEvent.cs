using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class ShopItemCreatedDomainEvent : DomainEvent
{
    public ShopItemCreatedDomainEvent(Guid shopItemId, Guid categoryId)
    {
        ShopItemId = shopItemId;
        CategoryId = categoryId;
    }

    public Guid ShopItemId { get; }
    public Guid CategoryId { get; }
}
