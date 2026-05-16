using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Players.IntegrationEvents;

public sealed record AuthProviderLinkedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PlayerId,
    string Provider,
    string ExternalId) : IIntegrationEvent;
