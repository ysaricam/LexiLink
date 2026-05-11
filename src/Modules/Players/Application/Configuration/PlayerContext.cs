using LexiLink.Common.Application;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Configuration;

public class PlayerContext : IPlayerContext
{
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public PlayerContext(IExecutionContextAccessor executionContextAccessor)
    {
        _executionContextAccessor = executionContextAccessor;
    }

    public PlayerId PlayerId => new(_executionContextAccessor.UserId);
}
