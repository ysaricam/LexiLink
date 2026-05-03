using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;

public class GetCategoryDetailsQuery : QueryBase<CategoryDetailsDto>
{
    public Guid CategoryId { get; }

    public GetCategoryDetailsQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
