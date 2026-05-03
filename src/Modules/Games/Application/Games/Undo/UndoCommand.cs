using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.Undo;

public class UndoCommand : CommandBase
{
    public UndoCommand(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; }
}
