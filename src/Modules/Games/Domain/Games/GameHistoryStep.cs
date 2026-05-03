using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games;

public sealed class GameHistoryStep : ValueObject
{
    public int StepNumber { get; }
    public LinkId LinkId { get; }

    private GameHistoryStep() { }

    internal GameHistoryStep(int stepNumber, LinkId linkId)
    {
        StepNumber = stepNumber;
        LinkId = linkId;
    }
}
