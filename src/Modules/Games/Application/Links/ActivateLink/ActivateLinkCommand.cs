using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.ActivateLink;

public class ActivateLinkCommand : CommandBase
{
    public Guid LinkId { get; }

    public ActivateLinkCommand(Guid linkId)
    {
        LinkId = linkId;
    }
}
