using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Configuration.CrossModule;
using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Application.Games.UseHint;

internal class UseHintCommandHandler : ICommandHandler<UseHintCommand, HintResultDto>
{
    private readonly IGameRepository _gameRepository;
    private readonly IHintGuard _hintGuard;
    private readonly ILinkNeighborResolver _neighborResolver;

    internal UseHintCommandHandler(
        IGameRepository gameRepository,
        IHintGuard hintGuard,
        ILinkNeighborResolver neighborResolver)
    {
        _gameRepository = gameRepository;
        _hintGuard = hintGuard;
        _neighborResolver = neighborResolver;
    }

    public async Task<HintResultDto> Handle(UseHintCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        // Free per-game allowance (set to 0 post-UR1 — kept for legacy
        // callers / tests) is consumed first. When exhausted, fall
        // through to the player's persistent inventory via the
        // IHintGuard sync gateway — Hint module decrements 1 charge
        // or throws HintBalanceMustBeSufficientRule when empty. Game
        // state advances either way through identical puzzle logic.
        HintResult hintResult;
        if (game.HasFreeHintRemaining)
        {
            hintResult = game.UseHint(_neighborResolver);
        }
        else
        {
            await _hintGuard.EnsureHintAvailableAsync(game.PlayerId, cancellationToken);
            hintResult = game.UseHintWithExternalInventory(_neighborResolver);
        }

        return new HintResultDto(hintResult.Type, hintResult.RecommendedLinkId.Value);
    }
}
