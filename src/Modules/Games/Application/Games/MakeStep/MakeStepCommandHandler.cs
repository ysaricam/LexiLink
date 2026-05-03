using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Application.Games.MakeStep;

internal class MakeStepCommandHandler : ICommandHandler<MakeStepCommand>
{
    private readonly IGameRepository _gameRepository;
    private readonly ILinkNeighborResolver _neighborResolver;
    private readonly IScoreCalculator _scoreCalculator;

    internal MakeStepCommandHandler(
        IGameRepository gameRepository,
        ILinkNeighborResolver neighborResolver,
        IScoreCalculator scoreCalculator)
    {
        _gameRepository = gameRepository;
        _neighborResolver = neighborResolver;
        _scoreCalculator = scoreCalculator;
    }

    public async Task Handle(MakeStepCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        game.MakeStep(new LinkId(request.NextLinkId), _neighborResolver, _scoreCalculator);
    }
}
