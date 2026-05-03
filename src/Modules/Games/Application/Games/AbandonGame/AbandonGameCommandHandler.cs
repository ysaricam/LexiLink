using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.AbandonGame;

internal class AbandonGameCommandHandler : ICommandHandler<AbandonGameCommand>
{
    private readonly IGameRepository _gameRepository;

    internal AbandonGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task Handle(AbandonGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        game.Abandon();
    }
}
