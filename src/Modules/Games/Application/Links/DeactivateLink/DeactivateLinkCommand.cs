using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.DeactivateLink;

public class DeactivateLinkCommand : CommandBase
{
    public Guid LinkId { get; }

    public DeactivateLinkCommand(Guid linkId)
    {
        LinkId = linkId;
    }
}
