using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Players.RegisterGuestPlayer;

public class RegisterGuestPlayerCommand : CommandBase<Guid>
{
    public string DeviceId { get; }
    public string DisplayName { get; }
    public string Locale { get; }

    public RegisterGuestPlayerCommand(string deviceId, string displayName, string locale)
    {
        DeviceId = deviceId;
        DisplayName = displayName;
        Locale = locale;
    }
}
