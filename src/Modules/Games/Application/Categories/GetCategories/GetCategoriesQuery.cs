using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.GetCategories;

public class GetCategoriesQuery : QueryBase<List<CategoryListItemDto>>
{
    public GetCategoriesQuery(string? locale = null)
    {
        Locale = locale;
    }

    public string? Locale { get; }
}
