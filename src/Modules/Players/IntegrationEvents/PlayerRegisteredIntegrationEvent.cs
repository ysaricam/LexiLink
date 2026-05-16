using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Players.IntegrationEvents;

public sealed record PlayerRegisteredIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PlayerId,
    string DisplayName,
    int Discriminator,
    string Locale,
    bool IsGuest) : IIntegrationEvent;
