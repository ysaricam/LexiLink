using LexiLink.Modules.Players.Domain.Players.Events;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Tests.Players;

[TestFixture]
public class PlayerUpdateProfileTests : PlayerTestsBase
{
    [Test]
    public void UpdateProfile_WithValidValues_RaisesPlayerProfileUpdatedDomainEvent()
    {
        var player = RegisterGuest();

        player.UpdateProfile("https://example.com/avatar.png", "en-US");

        AssertPublishedDomainEvent<PlayerProfileUpdatedDomainEvent>(player)
            .PlayerId.Should().Be(player.Id);
    }

    [Test]
    public void UpdateProfile_WhenAvatarUrlIsNull_IsSuccessful()
    {
        var player = RegisterGuest();
        player.UpdateProfile(avatarUrl: null, "en-US");
        AssertPublishedDomainEvent<PlayerProfileUpdatedDomainEvent>(player);
    }

    [Test]
    public void UpdateProfile_WhenAvatarUrlIsNonHttp_BreaksAvatarUrlMustBeValidIfProvidedRule()
    {
        var player = RegisterGuest();
        AssertBrokenRule<AvatarUrlMustBeValidIfProvidedRule>(() =>
            player.UpdateProfile("ftp://example.com/avatar.png", "en-US"));
    }

    [Test]
    public void UpdateProfile_WhenAvatarUrlIsMalformed_BreaksAvatarUrlMustBeValidIfProvidedRule()
    {
        var player = RegisterGuest();
        AssertBrokenRule<AvatarUrlMustBeValidIfProvidedRule>(() =>
            player.UpdateProfile("not-a-url", "en-US"));
    }

    [Test]
    public void UpdateProfile_WhenAvatarUrlExceedsMaxLength_BreaksAvatarUrlMustBeValidIfProvidedRule()
    {
        var player = RegisterGuest();
        var tooLongAvatarUrl = "https://example.com/" + new string('a', AvatarUrlMustBeValidIfProvidedRule.MaxLength);

        AssertBrokenRule<AvatarUrlMustBeValidIfProvidedRule>(() =>
            player.UpdateProfile(tooLongAvatarUrl, "en-US"));
    }

    [Test]
    public void UpdateProfile_WhenLocaleFormatIsInvalid_BreaksLocaleMustBeValidFormatRule()
    {
        var player = RegisterGuest();
        AssertBrokenRule<LocaleMustBeValidFormatRule>(() =>
            player.UpdateProfile(null, "english"));
    }
}
