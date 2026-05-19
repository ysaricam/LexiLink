using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Administration.IntegrationEvents;

public sealed record AdminUserRegisteredIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    Guid AdminUserId,
    string Email,
    string Role) : IIntegrationEvent;
