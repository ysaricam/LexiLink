using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class PlayerMustNotAlreadyHaveAuthProviderRule : IBusinessRule
{
    private readonly AuthProvider _provider;
    private readonly IReadOnlyCollection<AuthIdentity> _existing;

    public PlayerMustNotAlreadyHaveAuthProviderRule(AuthProvider provider, IReadOnlyCollection<AuthIdentity> existing)
    {
        _provider = provider;
        _existing = existing;
    }

    public bool IsBroken() => _existing.Any(i => i.Provider == _provider);

    public string Message => $"Player already has an auth identity for provider '{_provider}'.";
}
