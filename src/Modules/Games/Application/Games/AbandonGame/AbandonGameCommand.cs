using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.AbandonGame;

public class AbandonGameCommand : CommandBase
{
    public AbandonGameCommand(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; }
}
