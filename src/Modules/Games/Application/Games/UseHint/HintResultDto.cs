using LexiLink.Modules.Games.Domain.Games.Puzzles;

namespace LexiLink.Modules.Games.Application.Games.UseHint;

public record HintResultDto(HintType Type, Guid RecommendedLinkId);
