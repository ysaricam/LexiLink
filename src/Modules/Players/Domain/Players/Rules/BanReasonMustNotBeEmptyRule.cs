using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class BanReasonMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _reason;

    public BanReasonMustNotBeEmptyRule(string? reason)
    {
        _reason = reason;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_reason);

    public string Message => "Ban reason is required so audit log entries are meaningful.";
}
