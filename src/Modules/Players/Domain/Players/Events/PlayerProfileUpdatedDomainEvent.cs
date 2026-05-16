using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerProfileUpdatedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }
    public string? AvatarUrl { get; }
    public string Locale { get; }

    public PlayerProfileUpdatedDomainEvent(PlayerId playerId, string? avatarUrl, string locale)
    {
        PlayerId = playerId;
        AvatarUrl = avatarUrl;
        Locale = locale;
    }
}
