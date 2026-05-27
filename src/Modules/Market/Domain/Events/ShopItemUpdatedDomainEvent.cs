using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class ShopItemUpdatedDomainEvent : DomainEvent
{
    public ShopItemUpdatedDomainEvent(Guid shopItemId)
    {
        ShopItemId = shopItemId;
    }

    public Guid ShopItemId { get; }
}
