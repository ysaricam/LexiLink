using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.Undo;

internal class UndoCommandHandler : ICommandHandler<UndoCommand>
{
    private readonly IGameRepository _gameRepository;

    internal UndoCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task Handle(UndoCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        game.Undo();
    }
}
