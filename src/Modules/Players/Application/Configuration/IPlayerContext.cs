using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Configuration;

public interface IPlayerContext
{
    PlayerId PlayerId { get; }
}
