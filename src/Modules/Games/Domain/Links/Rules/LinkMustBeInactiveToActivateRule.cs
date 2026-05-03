using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkMustBeInactiveToActivateRule : IBusinessRule
{
    private readonly bool _isActive;

    public LinkMustBeInactiveToActivateRule(bool isActive)
    {
        _isActive = isActive;
    }

    public bool IsBroken() => _isActive;

    public string Message => "Link must be inactive to be activated.";
}
