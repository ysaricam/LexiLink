using LexiLink.Common.Domain;

namespace LexiLink.Modules.Players.Domain.Players.Rules;

public class AvatarUrlMustBeValidIfProvidedRule : IBusinessRule
{
    public const int MaxLength = 500;

    private readonly string? _avatarUrl;

    public AvatarUrlMustBeValidIfProvidedRule(string? avatarUrl)
    {
        _avatarUrl = avatarUrl;
    }

    public bool IsBroken()
    {
        if (string.IsNullOrEmpty(_avatarUrl))
            return false;

        if (_avatarUrl.Length > MaxLength)
            return true;

        return !Uri.TryCreate(_avatarUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps);
    }

    public string Message => $"Avatar url must be a valid http(s) URL no longer than {MaxLength} characters.";
}
