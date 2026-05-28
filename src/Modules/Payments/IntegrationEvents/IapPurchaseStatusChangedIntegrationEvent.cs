using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Payments.IntegrationEvents;

public sealed record IapPurchaseStatusChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid IapPurchaseId,
    string Status) : IIntegrationEvent;
