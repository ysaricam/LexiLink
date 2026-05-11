using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Players.UpdatePlayerProfile;

public class UpdatePlayerProfileCommand : CommandBase
{
    public Guid PlayerId { get; }
    public string? AvatarUrl { get; }
    public string Locale { get; }

    public UpdatePlayerProfileCommand(Guid playerId, string? avatarUrl, string locale)
    {
        PlayerId = playerId;
        AvatarUrl = avatarUrl;
        Locale = locale;
    }
}
