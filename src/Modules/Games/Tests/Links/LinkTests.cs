using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Links.Events;

namespace LexiLink.Modules.Games.Tests.Links;

[TestFixture]
public class LinkTests : LinkTestsBase
{
    [Test]
    public void Create_WithValidValues_RaisesLinkCreatedDomainEvent()
    {
        var categoryId = NewCategoryId();

        var link = Link.Create(categoryId, "cat", "feline", isActive: true);

        link.Should().NotBeNull();
        link.Id.Should().NotBeNull();
        link.CategoryId.Should().Be(categoryId);
        link.OutgoingLinkIds.Should().BeEmpty();
        AssertPublishedDomainEvent<LinkCreatedDomainEvent>(link)
            .LinkId.Should().Be(link.Id);
    }

    [Test]
    public void Create_WithIsActiveFalse_StartsInactive()
    {
        var link = Link.Create(NewCategoryId(), "cat", "", isActive: false);

        // Cannot be deactivated again — proves it started inactive.
        AssertBrokenRule<Domain.Links.Rules.LinkMustBeActiveToDeactivateRule>(link.Deactivate);
    }
}
