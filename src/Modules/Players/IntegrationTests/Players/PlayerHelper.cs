using LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;
using MediatR;

namespace LexiLink.Modules.Players.IntegrationTests.Players;

internal static class PlayerHelper
{
    public const string DeviceId = "device-integration-001";
    public const string DisplayName = "Yasin";
    public const string Locale = "tr-TR";

    public static Task<Guid> RegisterGuestPlayerAsync(
        ISender sender,
        string deviceId = DeviceId,
        string displayName = DisplayName,
        string locale = Locale)
    {
        return sender.Send(new RegisterGuestPlayerCommand(deviceId, displayName, locale));
    }
}
