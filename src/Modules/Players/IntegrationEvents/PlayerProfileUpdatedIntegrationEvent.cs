using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Players.IntegrationEvents;

public sealed record PlayerProfileUpdatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid PlayerId,
    string? AvatarUrl,
    string Locale) : IIntegrationEvent;
