using LexiLink.Modules.Games.Domain.Games;

namespace LexiLink.Modules.Games.Application.Games.GetGameById;

public record GameDetailsDto(
    Guid Id,
    Guid PlayerId,
    Guid CategoryId,
    Difficulty Difficulty,
    Guid StartLinkId,
    string StartWord,
    Guid TargetLinkId,
    string TargetWord,
    string TargetDescription,
    Guid CurrentLinkId,
    string CurrentWord,
    string CurrentDescription,
    GameState State,
    int? Score,
    int MaxSteps,
    int StepsTaken,
    int HintsTotal,
    int HintsUsed,
    int UndosTotal,
    int UndosUsed,
    int ResetsTotal,
    int ResetsUsed)
{
    public IReadOnlyList<GameHistoryStepDto> History { get; init; } = [];
}
