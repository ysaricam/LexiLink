using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.StartGame;

public class StartGameCommand : CommandBase
{
    public StartGameCommand(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; }
}
