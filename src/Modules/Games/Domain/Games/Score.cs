using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games;

public class Score : ValueObject
{
    public int Points { get; }

    private Score() { }

    private Score(int points) => Points = points;

    public static Score Of(int points) => new(points);
}
