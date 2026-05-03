using LexiLink.Modules.Games.Application.Contracts;

namespace LexiLink.Modules.Games.Application.Links.GetLinksByCategory;

public class GetLinksByCategoryQuery : QueryBase<List<LinkListItemDto>>
{
    public Guid CategoryId { get; }

    public GetLinksByCategoryQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
