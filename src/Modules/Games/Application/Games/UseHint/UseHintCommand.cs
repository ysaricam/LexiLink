using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Games.UseHint;

public class UseHintCommand : CommandBase<HintResultDto>
{
    public UseHintCommand(Guid gameId)
    {
        GameId = gameId;
    }

    public Guid GameId { get; }
}
