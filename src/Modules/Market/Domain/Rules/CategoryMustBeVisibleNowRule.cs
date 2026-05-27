using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class CategoryMustBeVisibleNowRule : IBusinessRule
{
    private readonly Category _category;
    private readonly DateTime _now;

    internal CategoryMustBeVisibleNowRule(Category category, DateTime now)
    {
        _category = category;
        _now = now;
    }

    public bool IsBroken() => !_category.IsVisibleAt(_now);

    public string Message => "Category must be visible now.";
}
