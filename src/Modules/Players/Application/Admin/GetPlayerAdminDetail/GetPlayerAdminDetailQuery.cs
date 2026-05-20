using LexiLink.Modules.Players.Application.Contracts;

namespace LexiLink.Modules.Players.Application.Admin.GetPlayerAdminDetail;

public sealed class GetPlayerAdminDetailQuery : QueryBase<PlayerAdminDetailDto?>
{
    public Guid PlayerId { get; }

    public GetPlayerAdminDetailQuery(Guid playerId)
    {
        PlayerId = playerId;
    }
}
