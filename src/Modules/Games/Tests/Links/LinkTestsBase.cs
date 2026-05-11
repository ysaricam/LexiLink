using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Links;

public abstract class LinkTestsBase : TestBase
{
    protected static CategoryId NewCategoryId() => new(Guid.NewGuid());
    protected static LinkId NewLinkId() => new(Guid.NewGuid());

    protected static Link CreateLink(
        CategoryId? categoryId = null,
        string value = "cat",
        string description = "",
        bool isActive = true)
    {
        var link = Link.Create(categoryId ?? NewCategoryId(), value, description, isActive);
        DomainEventsTestHelper.ClearAllDomainEvents(link);
        return link;
    }
}
