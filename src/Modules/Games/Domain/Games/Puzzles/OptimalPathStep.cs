using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Puzzles;

public sealed class OptimalPathStep : ValueObject
{
    public int Position { get; }
    public LinkId LinkId { get; } = default!;

    private OptimalPathStep() { }

    internal OptimalPathStep(int position, LinkId linkId)
    {
        Position = position;
        LinkId = linkId;
    }
}
