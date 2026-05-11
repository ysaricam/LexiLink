using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class DeviceIdMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _deviceId;

    public DeviceIdMustNotBeEmptyRule(string? deviceId)
    {
        _deviceId = deviceId;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_deviceId);

    public string Message => "Device id cannot be empty when registering a guest player.";
}
