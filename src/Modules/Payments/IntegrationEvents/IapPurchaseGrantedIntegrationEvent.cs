using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Payments.IntegrationEvents;

public sealed record IapPurchaseGrantedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid IapPurchaseId,
    Guid PlayerId,
    string Platform,
    string StoreProductId,
    int DiamondAmount,
    DateTime GrantedAt) : IIntegrationEvent;
