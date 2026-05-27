using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.Admin.ResetPlayerDiamond;

internal sealed class ResetPlayerDiamondCommandHandler : ICommandHandler<ResetPlayerDiamondCommand>
{
    private readonly IPlayerDiamondInventoryRepository _repository;
    private readonly IClock _clock;

    internal ResetPlayerDiamondCommandHandler(IPlayerDiamondInventoryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ResetPlayerDiamondCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerDiamondInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerDiamondInventory), request.PlayerId);

        inventory.AdminReset(_clock.UtcNow);
    }
}
