using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.MakeStep;

public class MakeStepCommand : CommandBase
{
    public MakeStepCommand(Guid gameId, Guid nextLinkId)
    {
        GameId = gameId;
        NextLinkId = nextLinkId;
    }

    public Guid GameId { get; }
    public Guid NextLinkId { get; }
}
