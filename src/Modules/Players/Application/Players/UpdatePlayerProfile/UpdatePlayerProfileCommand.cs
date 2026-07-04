using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;

public class UpdatePlayerProfileCommand : CommandBase
{
    public Guid PlayerId { get; }
    public string? AvatarUrl { get; }
    public string Locale { get; }
    public string? DisplayName { get; }
    public int? Discriminator { get; }

    public UpdatePlayerProfileCommand(
        Guid playerId,
        string? avatarUrl,
        string locale,
        string? displayName = null,
        int? discriminator = null)
    {
        PlayerId = playerId;
        AvatarUrl = avatarUrl;
        Locale = locale;
        DisplayName = displayName;
        Discriminator = discriminator;
    }
}
