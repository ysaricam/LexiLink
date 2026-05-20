using LexiLink.Modules.Players.Application.Configuration.Queries;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Admin.GetPlayerBanStatus;

internal sealed class GetPlayerBanStatusQueryHandler : IQueryHandler<GetPlayerBanStatusQuery, bool>
{
    private readonly IPlayerRepository _repository;

    internal GetPlayerBanStatusQueryHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(GetPlayerBanStatusQuery request, CancellationToken cancellationToken)
    {
        var player = await _repository.GetByIdAsync(
            new PlayerId(request.PlayerId),
            cancellationToken);
        return player?.IsBanned ?? false;
    }
}
