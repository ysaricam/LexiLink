using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Events;

public class PlayerRegisteredDomainEvent : DomainEvent
{
    public PlayerId PlayerId { get; }
    public string DisplayName { get; }
    public Discriminator Discriminator { get; }
    public string Locale { get; }
    public bool IsGuest { get; }

    public PlayerRegisteredDomainEvent(
        PlayerId playerId,
        string displayName,
        Discriminator discriminator,
        string locale,
        bool isGuest)
    {
        PlayerId = playerId;
        DisplayName = displayName;
        Discriminator = discriminator;
        Locale = locale;
        IsGuest = isGuest;
    }
}
