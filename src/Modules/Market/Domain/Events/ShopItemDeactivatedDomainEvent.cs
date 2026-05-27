using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Events;

public sealed class ShopItemDeactivatedDomainEvent : DomainEvent
{
    public ShopItemDeactivatedDomainEvent(Guid shopItemId)
    {
        ShopItemId = shopItemId;
    }

    public Guid ShopItemId { get; }
}
