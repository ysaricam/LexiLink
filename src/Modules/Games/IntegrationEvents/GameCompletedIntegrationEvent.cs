using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Games.IntegrationEvents;

public sealed record GameCompletedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid GameId,
    Guid PlayerId,
    Guid StartLinkId,
    Guid TargetLinkId,
    int Score) : IIntegrationEvent;
