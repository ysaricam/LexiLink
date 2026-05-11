using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Domain.Links.Rules;

public class LinkOutgoingMustBeSameCategoryRule : IBusinessRule
{
    private readonly CategoryId _sourceCategoryId;
    private readonly CategoryId _targetCategoryId;

    public LinkOutgoingMustBeSameCategoryRule(CategoryId sourceCategoryId, CategoryId targetCategoryId)
    {
        _sourceCategoryId = sourceCategoryId;
        _targetCategoryId = targetCategoryId;
    }

    public bool IsBroken() => _sourceCategoryId != _targetCategoryId;

    public string Message => "Outgoing link must belong to the same category as the source link.";
}
