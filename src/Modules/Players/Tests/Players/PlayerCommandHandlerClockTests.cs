using LexiLink.Common.Application.Time;
using LexiLink.Modules.Players.Application.Players.LinkAuthProvider;
using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using LexiLink.Modules.Players.Domain.Players;
using NSubstitute;

namespace LexiLink.Modules.Players.Tests.Players;

[TestFixture]
public class PlayerCommandHandlerClockTests : PlayerTestsBase
{
    private static readonly DateTime FixedNow = new(2026, 5, 12, 20, 30, 0, DateTimeKind.Utc);

    [Test]
    public async Task RegisterGuestPlayerCommandHandler_Should_Use_Clock_For_Registration_Time()
    {
        var repository = Substitute.For<IPlayerRepository>();
        var discriminatorGenerator = Substitute.For<IDiscriminatorGenerator>();
        discriminatorGenerator.GenerateForAsync(ValidDisplayName, Arg.Any<CancellationToken>())
            .Returns(NewDiscriminator());
        var clock = new FixedClock(FixedNow);
        Player? addedPlayer = null;
        repository.AddAsync(Arg.Do<Player>(player => addedPlayer = player), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new RegisterGuestPlayerCommandHandler(
            repository,
            discriminatorGenerator,
            clock);

        await handler.Handle(
            new RegisterGuestPlayerCommand(ValidDeviceId, ValidDisplayName, ValidLocale),
            CancellationToken.None);

        addedPlayer.Should().NotBeNull();
        addedPlayer!.AuthIdentities.Single().LinkedAt.Should().Be(FixedNow);
    }

    [Test]
    public async Task LinkAuthProviderCommandHandler_Should_Use_Clock_For_Linked_Time()
    {
        var player = RegisterGuest();
        var repository = Substitute.For<IPlayerRepository>();
        repository.GetByIdAsync(player.Id, Arg.Any<CancellationToken>())
            .Returns(player);
        var clock = new FixedClock(FixedNow);
        var handler = new LinkAuthProviderCommandHandler(repository, clock);

        await handler.Handle(
            new LinkAuthProviderCommand(
                player.Id.Value,
                AuthProvider.Apple,
                "apple-sub",
                "yasin@example.com"),
            CancellationToken.None);

        player.AuthIdentities.Single(identity => identity.Provider == AuthProvider.Apple)
            .LinkedAt.Should().Be(FixedNow);
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
