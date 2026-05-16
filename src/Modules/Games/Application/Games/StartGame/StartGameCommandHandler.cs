using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.StartGame;

internal class StartGameCommandHandler : ICommandHandler<StartGameCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly IEnergyGuard _energyGuard;

    internal StartGameCommandHandler(IGameRepository gameRepository, IEnergyGuard energyGuard)
    {
        _gameRepository = gameRepository;
        _energyGuard = energyGuard;
    }

    public async Task Handle(StartGameCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        // Energy is consumed before the state transition so insufficient-energy callers
        // never advance Initial state. Residual dual-write risk: if game.Start() throws
        // (e.g. game already started) the energy is already debited. Acceptable for MVP
        // — only happens on duplicate StartGame after a successful one.
        await _energyGuard.EnsureCanStartGameAsync(game.PlayerId, cancellationToken);

        game.Start();
    }
}
