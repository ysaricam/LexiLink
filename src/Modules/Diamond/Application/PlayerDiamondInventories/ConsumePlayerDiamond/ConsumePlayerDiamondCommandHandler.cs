using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.Commands;
using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;

namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.ConsumePlayerDiamond;

internal class ConsumePlayerDiamondCommandHandler : ICommandHandler<ConsumePlayerDiamondCommand>
{
    private readonly IPlayerDiamondInventoryRepository _repository;
    private readonly IClock _clock;

    internal ConsumePlayerDiamondCommandHandler(
        IPlayerDiamondInventoryRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ConsumePlayerDiamondCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerDiamondInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerDiamondInventory), request.PlayerId);

        inventory.Consume(request.Amount, _clock.UtcNow);
    }
}
