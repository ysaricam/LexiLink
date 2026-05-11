using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Players.GetPlayerById;

public class GetPlayerByIdQuery : QueryBase<PlayerDetailsDto>
{
    public Guid PlayerId { get; }

    public GetPlayerByIdQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
