using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.UseHint;

internal class UseHintCommandHandler : ICommandHandler<UseHintCommand, HintResultDto>
{
    private readonly IGameRepository _gameRepository;

    internal UseHintCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<HintResultDto> Handle(UseHintCommand request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(new GameId(request.GameId), cancellationToken)
            ?? throw new NotFoundException(nameof(Game), request.GameId);

        var hintResult = game.UseHint();

        return new HintResultDto(hintResult.Type, hintResult.RecommendedLinkId.Value);
    }
}
