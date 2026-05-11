using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.Domain.Players.Events;
using LexiLink.Modules.Players.Domain.Players.Rules;

namespace LexiLink.Modules.Players.Tests.Players;

[TestFixture]
public class PlayerRegisterTests : PlayerTestsBase
{
    [Test]
    public void RegisterGuest_WithValidValues_IsSuccessful()
    {
        var player = Player.RegisterGuest(
            ValidDeviceId, ValidDisplayName, NewDiscriminator(), ValidLocale, FixedRegisteredAt);

        player.Should().NotBeNull();
        player.Id.Should().NotBeNull();
        player.AuthIdentities.Should().HaveCount(1);
        player.AuthIdentities.Single().Provider.Should().Be(AuthProvider.Guest);
        player.AuthIdentities.Single().ExternalId.Should().Be(ValidDeviceId);
        player.AuthIdentities.Single().Email.Should().BeNull();
        player.AuthIdentities.Single().LinkedAt.Should().Be(FixedRegisteredAt);
    }

    [Test]
    public void RegisterGuest_RaisesPlayerRegisteredDomainEvent_WithIsGuestTrue()
    {
        var player = Player.RegisterGuest(
            ValidDeviceId, ValidDisplayName, NewDiscriminator(7777), ValidLocale, FixedRegisteredAt);

        var evt = AssertPublishedDomainEvent<PlayerRegisteredDomainEvent>(player);
        evt.PlayerId.Should().Be(player.Id);
        evt.DisplayName.Should().Be(ValidDisplayName);
        evt.Discriminator.Value.Should().Be(7777);
        evt.IsGuest.Should().BeTrue();
    }

    [Test]
    public void RegisterGuest_WhenDeviceIdIsEmpty_BreaksDeviceIdMustNotBeEmptyRule()
    {
        AssertBrokenRule<DeviceIdMustNotBeEmptyRule>(() =>
            Player.RegisterGuest("", ValidDisplayName, NewDiscriminator(), ValidLocale, FixedRegisteredAt));
    }

    [Test]
    public void RegisterGuest_WhenDisplayNameIsEmpty_BreaksDisplayNameMustNotBeEmptyRule()
    {
        AssertBrokenRule<DisplayNameMustNotBeEmptyRule>(() =>
            Player.RegisterGuest(ValidDeviceId, "", NewDiscriminator(), ValidLocale, FixedRegisteredAt));
    }

    [Test]
    public void RegisterGuest_WhenDisplayNameIsTooShort_BreaksDisplayNameMustMeetMinimumLengthRule()
    {
        AssertBrokenRule<DisplayNameMustMeetMinimumLengthRule>(() =>
            Player.RegisterGuest(ValidDeviceId, "Y", NewDiscriminator(), ValidLocale, FixedRegisteredAt));
    }

    [Test]
    public void RegisterGuest_WhenDisplayNameExceedsMaxLength_BreaksDisplayNameMustNotExceedMaxLengthRule()
    {
        var tooLong = new string('a', DisplayNameMustNotExceedMaxLengthRule.MaxLength + 1);
        AssertBrokenRule<DisplayNameMustNotExceedMaxLengthRule>(() =>
            Player.RegisterGuest(ValidDeviceId, tooLong, NewDiscriminator(), ValidLocale, FixedRegisteredAt));
    }

    [Test]
    public void RegisterGuest_WhenLocaleFormatIsInvalid_BreaksLocaleMustBeValidFormatRule()
    {
        AssertBrokenRule<LocaleMustBeValidFormatRule>(() =>
            Player.RegisterGuest(ValidDeviceId, ValidDisplayName, NewDiscriminator(), "turkish", FixedRegisteredAt));
    }
}
