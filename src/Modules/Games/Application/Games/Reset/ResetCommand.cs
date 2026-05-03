using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.Reset;

public class ResetCommand : CommandBase
{
    public ResetCommand(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; }
}
