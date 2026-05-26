using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.Reset;

internal class ResetCommandHandler : ICommandHandler<ResetCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IResetGuard _resetGuard;

    internal ResetCommandHandler(
        IGameRepository gameRepository,
        IResetGuard resetGuard)
    {
        _gameRepository = gameRepository;
        _resetGuard = resetGuard;
    }

    public async Task Handle(ResetCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        await _resetGuard.EnsureResetAvailableAsync(game.PlayerId, cancellationToken);
        game.ResetWithExternalInventory();
    }
}
