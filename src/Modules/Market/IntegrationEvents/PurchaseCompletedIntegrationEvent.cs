using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Market.IntegrationEvents;

public sealed record PurchaseCompletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PlayerId,
    Guid PurchaseOrderId,
    Guid ShopItemId,
    string ItemType,
    int Quantity,
    int DiamondsPaid,
    DateTime PurchasedAt,
    string IdempotencyKey) : IIntegrationEvent;
