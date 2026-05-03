using LexiLink.Modules.Games.Application.Contracts;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.CreateGame;

public class CreateGameCommand : CommandBase<Guid>
{
    public CreateGameCommand(Guid playerId, Guid categoryId, Difficulty difficulty)
    {
        PlayerId = playerId;
        CategoryId = categoryId;
        Difficulty = difficulty;
    }

    public Guid PlayerId { get; }
    public Guid CategoryId { get; }
    public Difficulty Difficulty { get; }
}
