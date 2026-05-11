using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class ExternalAuthIdMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _externalId;

    public ExternalAuthIdMustNotBeEmptyRule(string? externalId)
    {
        _externalId = externalId;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_externalId);

    public string Message => "External auth id cannot be empty.";
}
