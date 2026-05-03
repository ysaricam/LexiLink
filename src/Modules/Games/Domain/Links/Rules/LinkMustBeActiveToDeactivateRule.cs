using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkMustBeActiveToDeactivateRule : IBusinessRule
{
    private readonly bool _isActive;

    public LinkMustBeActiveToDeactivateRule(bool isActive)
    {
        _isActive = isActive;
    }

    public bool IsBroken() => !_isActive;

    public string Message => "Link must be active to be deactivated.";
}
