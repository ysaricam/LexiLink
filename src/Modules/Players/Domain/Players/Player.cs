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
    private bool _isBanned;
    private string? _bannedReason;
    private DateTime? _bannedAt;

    private readonly List<AuthIdentity> _authIdentities;

    public IReadOnlyCollection<AuthIdentity> AuthIdentities => _authIdentities.AsReadOnly();

    public string DisplayName => _displayName;
    public Discriminator Discriminator => _discriminator;
    public string? AvatarUrl => _avatarUrl;
    public string Locale => _locale;
    public DateTime CreatedAt => _createdAt;
    public bool IsGuest => _isGuest;
    public bool IsBanned => _isBanned;
    public string? BannedReason => _bannedReason;
    public DateTime? BannedAt => _bannedAt;

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

    /// <summary>
    /// Admin force-ban. Reason is mandatory so audit log entries are
    /// meaningful. Idempotent — re-banning an already-banned player is
    /// a no-op (no second event). To change the reason, unban then ban.
    /// The auth boundary refuses tokens that map to a banned player.
    /// </summary>
    public void Ban(string reason, DateTime now)
    {
        CheckRule(new BanReasonMustNotBeEmptyRule(reason));

        if (_isBanned)
        {
            return;
        }

        _isBanned = true;
        _bannedReason = reason.Trim();
        _bannedAt = now;

        AddDomainEvent(new PlayerBannedDomainEvent(Id, _bannedReason));
    }

    /// <summary>
    /// Admin lift-ban. Idempotent for already-unbanned players.
    /// </summary>
    public void Unban(DateTime now)
    {
        if (!_isBanned)
        {
            return;
        }

        _isBanned = false;
        _bannedReason = null;
        _bannedAt = null;

        AddDomainEvent(new PlayerUnbannedDomainEvent(Id));
    }
}
