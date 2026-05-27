using LexiLink.Common.Domain;
using LexiLink.Modules.Market.Domain.Rules;

namespace LexiLink.Modules.Market.Domain;

public sealed class VisibilityWindow : ValueObject
{
    public DateTime StartsAt { get; }
    public DateTime EndsAt { get; }

    private VisibilityWindow()
    {
    }

    private VisibilityWindow(DateTime startsAt, DateTime endsAt)
    {
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public static VisibilityWindow Create(DateTime startsAt, DateTime endsAt)
    {
        CheckRule(new WindowMustBeOrderedRule(startsAt, endsAt));

        return new VisibilityWindow(startsAt, endsAt);
    }

    public bool IsOpenAt(DateTime now) => StartsAt <= now && now < EndsAt;

}
