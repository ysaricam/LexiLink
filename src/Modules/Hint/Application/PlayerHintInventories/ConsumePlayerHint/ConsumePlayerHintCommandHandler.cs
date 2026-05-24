using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Hint.Application.Configuration.Commands;
using LexiLink.Modules.Hint.Domain.PlayerHintInventories;

namespace LexiLink.Modules.Hint.Application.PlayerHintInventories.ConsumePlayerHint;

internal class ConsumePlayerHintCommandHandler : ICommandHandler<ConsumePlayerHintCommand>
{
    private readonly IPlayerHintInventoryRepository _repository;
    private readonly IClock _clock;

    internal ConsumePlayerHintCommandHandler(
        IPlayerHintInventoryRepository repository,
        IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task Handle(ConsumePlayerHintCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByIdAsync(
            new PlayerHintInventoryId(request.PlayerId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(PlayerHintInventory), request.PlayerId);

        inventory.Consume(request.Amount, _clock.UtcNow);
    }
}
