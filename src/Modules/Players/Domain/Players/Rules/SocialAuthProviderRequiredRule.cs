using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class SocialAuthProviderRequiredRule : IBusinessRule
{
    private readonly AuthProvider _provider;

    public SocialAuthProviderRequiredRule(AuthProvider provider)
    {
        _provider = provider;
    }

    public bool IsBroken() => _provider == AuthProvider.Guest;

    public string Message => "Only social providers (Apple, Google) can be linked; Guest is the initial state.";
}
