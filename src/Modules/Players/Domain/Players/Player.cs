using LexiLink.Common.Domain;
using LexiLink.Modules.Players.Domain.Players.Events;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Domain.Players;

public class Player : Entity, IAggregateRoot
{
    public PlayerId Id { get; private set; }

    private string _displayName;
    private Discriminator _discriminator;
    private string? _avatarUrl;
    private string _locale;
    private DateTime _createdAt;
    private bool _isGuest;

    private readonly List<AuthIdentity> _authIdentities;

    public IReadOnlyCollection<AuthIdentity> AuthIdentities => _authIdentities.AsReadOnly();

    private Player()
    {
        Id = null!;
        _displayName = null!;
        _discriminator = null!;
        _locale = null!;
        _authIdentities = [];
    }

    private Player(
        string deviceId,
        string displayName,
        Discriminator discriminator,
        string locale,
        DateTime registeredAt)
    {
        CheckRule(new DeviceIdMustNotBeEmptyRule(deviceId));
        CheckRule(new DisplayNameMustNotBeEmptyRule(displayName));
        CheckRule(new DisplayNameMustMeetMinimumLengthRule(displayName));
        CheckRule(new DisplayNameMustNotExceedMaxLengthRule(displayName));
        CheckRule(new LocaleMustBeValidFormatRule(locale));

        Id = new PlayerId(Guid.NewGuid());
        _displayName = displayName;
        _discriminator = discriminator;
        _avatarUrl = null;
        _locale = locale;
        _createdAt = registeredAt;
        _isGuest = true;

        _authIdentities = [new AuthIdentity(AuthProvider.Guest, deviceId, email: null, linkedAt: registeredAt)];

        AddDomainEvent(new PlayerRegisteredDomainEvent(Id, _displayName, _discriminator, _locale, _isGuest));
    }

    internal static Player RegisterGuest(
        string deviceId,
        string displayName,
        Discriminator discriminator,
        string locale,
        DateTime registeredAt)
    {
        return new Player(deviceId, displayName, discriminator, locale, registeredAt);
    }

    public void LinkAuthProvider(AuthProvider provider, string externalId, string? email, DateTime linkedAt)
    {
        CheckRule(new SocialAuthProviderRequiredRule(provider));
        CheckRule(new ExternalAuthIdMustNotBeEmptyRule(externalId));
        CheckRule(new PlayerMustNotAlreadyHaveAuthProviderRule(provider, _authIdentities));

        _authIdentities.Add(new AuthIdentity(provider, externalId, email, linkedAt));
        _isGuest = false;

        AddDomainEvent(new AuthProviderLinkedDomainEvent(Id, provider, externalId));
    }

    public void UpdateProfile(string? avatarUrl, string locale)
    {
        CheckRule(new AvatarUrlMustBeValidIfProvidedRule(avatarUrl));
        CheckRule(new LocaleMustBeValidFormatRule(locale));

        _avatarUrl = string.IsNullOrEmpty(avatarUrl) ? null : avatarUrl;
        _locale = locale;

        AddDomainEvent(new PlayerProfileUpdatedDomainEvent(Id, _avatarUrl, _locale));
    }
}
