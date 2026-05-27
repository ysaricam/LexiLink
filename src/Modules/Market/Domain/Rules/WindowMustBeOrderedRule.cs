using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class WindowMustBeOrderedRule : IBusinessRule
{
    private readonly DateTime _startsAt;
    private readonly DateTime _endsAt;

    internal WindowMustBeOrderedRule(DateTime startsAt, DateTime endsAt)
    {
        _startsAt = startsAt;
        _endsAt = endsAt;
    }

    public bool IsBroken() => _startsAt >= _endsAt;

    public string Message => "Window start must be before window end.";
}
