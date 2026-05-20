using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Domain.Players;

namespace LexiLink.Modules.Players.Application.Admin.BanPlayer;

internal sealed class BanPlayerCommandHandler : ICommandHandler<BanPlayerCommand>
{
    private readonly IPlayerRepository _repository;
    private readonly IClock _clock;

    internal BanPlayerCommandHandler(IPlayerRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(BanPlayerCommand request, CancellationToken cancellationToken)
    {
        var player = await _repository.GetByIdAsync(
            new PlayerId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Player), request.PlayerId);

        player.Ban(request.Reason, _clock.UtcNow);
    }
}
