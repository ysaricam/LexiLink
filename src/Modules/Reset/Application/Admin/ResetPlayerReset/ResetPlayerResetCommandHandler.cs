using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Reset.Application.Configuration.Commands;
using LexiLink.Modules.Reset.Domain.PlayerResetInventories;

namespace LexiLink.Modules.Reset.Application.Admin.ResetPlayerReset;

internal sealed class ResetPlayerResetCommandHandler : ICommandHandler<ResetPlayerResetCommand>
{
    private readonly IPlayerResetInventoryRepository _repository;
    private readonly IClock _clock;

    internal ResetPlayerResetCommandHandler(IPlayerResetInventoryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ResetPlayerResetCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerResetInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerResetInventory), request.PlayerId);

        inventory.AdminReset(_clock.UtcNow);
    }
}
