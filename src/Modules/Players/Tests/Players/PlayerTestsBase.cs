using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.Tests.SeedWork;

namespace LexiLink.Modules.Players.Tests.Players;

public abstract class PlayerTestsBase : TestBase
{
    protected const string ValidDeviceId = "device-abc-123";
    protected const string ValidDisplayName = "Yasin";
    protected const string ValidLocale = "tr-TR";

    protected static readonly DateTime FixedRegisteredAt = new(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
    protected static readonly DateTime FixedLinkedAt = new(2026, 5, 11, 10, 5, 0, DateTimeKind.Utc);

    protected static Discriminator NewDiscriminator(int value = 1234) => Discriminator.Of(value);

    protected static Player RegisterGuest(
        string deviceId = ValidDeviceId,
        string displayName = ValidDisplayName,
        int discriminatorValue = 1234,
        string locale = ValidLocale,
        DateTime? registeredAt = null,
        bool clearEvents = true)
    {
        var player = Player.RegisterGuest(
            deviceId,
            displayName,
            Discriminator.Of(discriminatorValue),
            locale,
            registeredAt ?? FixedRegisteredAt);

        if (clearEvents)
            DomainEventsTestHelper.ClearAllDomainEvents(player);

        return player;
    }
}
