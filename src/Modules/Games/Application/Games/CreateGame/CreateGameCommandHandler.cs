using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Application.Games.CreateGame;

internal class CreateGameCommandHandler : ICommandHandler<CreateGameCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILinkRepository _linkRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ICompletedGameLinkPairRepository _completedGameLinkPairRepository;
    private readonly IPathFinderService _pathFinder;
    private readonly IGameConfigurationService _gameConfiguration;
    private readonly Random _random;

    internal CreateGameCommandHandler(
        ICategoryRepository categoryRepository,
        ILinkRepository linkRepository,
        IGameRepository gameRepository,
        ICompletedGameLinkPairRepository completedGameLinkPairRepository,
        IPathFinderService pathFinder,
        IGameConfigurationService gameConfiguration,
        Random random)
    {
        _categoryRepository = categoryRepository;
        _linkRepository = linkRepository;
        _gameRepository = gameRepository;
        _completedGameLinkPairRepository = completedGameLinkPairRepository;
        _pathFinder = pathFinder;
        _gameConfiguration = gameConfiguration;
        _random = random;
    }

    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var linkIds = await _linkRepository.GetActiveIdsByCategoryAsync(category.Id, cancellationToken);
        var completedPairs = await _completedGameLinkPairRepository.GetCompletedPairsAsync(
            request.PlayerId,
            category.Id,
            cancellationToken);

        var puzzle = Puzzle.Create(
            category.Id,
            linkIds,
            request.Difficulty,
            _pathFinder,
            _gameConfiguration,
            _random,
            completedPairs);

        var maxSteps = _gameConfiguration.ResolveMaxSteps(request.Difficulty, puzzle.Depth);
        var hints = _gameConfiguration.ResolveHints(request.Difficulty);
        var undos = _gameConfiguration.ResolveUndos(request.Difficulty);
        var resets = _gameConfiguration.ResolveResets(request.Difficulty);

        var game = Game.Create(request.PlayerId, puzzle, maxSteps, hints, undos, resets);

        await _gameRepository.AddAsync(game, cancellationToken);

        return game.Id.Value;
    }
}
