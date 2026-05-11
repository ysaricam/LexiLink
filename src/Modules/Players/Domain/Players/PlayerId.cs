using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players;

public class PlayerId : TypedIdValueBase
{
    public PlayerId(Guid value) : base(value)
    {
    }
}
