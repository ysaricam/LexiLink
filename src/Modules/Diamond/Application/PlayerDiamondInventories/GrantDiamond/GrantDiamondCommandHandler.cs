using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GrantDiamond;

internal class GrantDiamondCommandHandler : ICommandHandler<GrantDiamondCommand>
{
    private readonly IPlayerDiamondInventoryRepository _repository;
    private readonly IClock _clock;

    internal GrantDiamondCommandHandler(
        IPlayerDiamondInventoryRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(GrantDiamondCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerDiamondInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerDiamondInventory), request.PlayerId);

        inventory.GrantBonus(request.Amount, _clock.UtcNow);
    }
}
