using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.Admin.SetPlayerDiamond;

internal sealed class SetPlayerDiamondCommandHandler : ICommandHandler<SetPlayerDiamondCommand>
{
    private readonly IPlayerDiamondInventoryRepository _repository;
    private readonly IClock _clock;

    internal SetPlayerDiamondCommandHandler(IPlayerDiamondInventoryRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(SetPlayerDiamondCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerDiamondInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerDiamondInventory), request.PlayerId);

        inventory.AdminSet(request.Balance, _clock.UtcNow);
    }
}
