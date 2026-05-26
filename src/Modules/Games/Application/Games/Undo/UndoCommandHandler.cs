using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.Undo;

internal class UndoCommandHandler : ICommandHandler<UndoCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUndoGuard _undoGuard;

    internal UndoCommandHandler(
        IGameRepository gameRepository,
        IUndoGuard undoGuard)
    {
        _gameRepository = gameRepository;
        _undoGuard = undoGuard;
    }

    public async Task Handle(UndoCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        await _undoGuard.EnsureUndoAvailableAsync(game.PlayerId, cancellationToken);
        game.UseUndoWithExternalInventory();
    }
}
