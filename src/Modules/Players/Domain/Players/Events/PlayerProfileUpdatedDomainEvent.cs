using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerProfileUpdatedDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }
    public string DisplayName { get; }
    public Discriminator Discriminator { get; }
    public string? AvatarUrl { get; }
    public string Locale { get; }

    public PlayerProfileUpdatedDomainEvent(
        PlayerId playerId,
        string displayName,
        Discriminator discriminator,
        string? avatarUrl,
        string locale)
    {
        PlayerId = playerId;
        DisplayName = displayName;
        Discriminator = discriminator;
        AvatarUrl = avatarUrl;
        Locale = locale;
    }
}
