using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class CategoryMustHaveEnoughLinksToStartGameRule : IBusinessRule
{
    private readonly IReadOnlyList<LinkId> _categoryLinksIds;
    private const int MinimumLinksRequired = 5;

    public CategoryMustHaveEnoughLinksToStartGameRule(IReadOnlyList<LinkId> categoryLinksIds)
    {
        _categoryLinksIds = categoryLinksIds;
    }

    public bool IsBroken() => _categoryLinksIds.Count < MinimumLinksRequired;

    public string Message => $"Category must have at least {MinimumLinksRequired} links to start a game.";
}
