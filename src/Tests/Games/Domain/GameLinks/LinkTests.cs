using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;
using LexiLink.Modules.Games.Domain.GameLinks.Rules;
using Xunit;

namespace LexiLink.Tests.Games.Domain.GameLinks;

public class LinkTests
{
    [Fact]
    public void Constructor_ShouldSetValue()
    {
        var link = Link.CreateNew("Fenerbahçe");
        Assert.Equal("Fenerbahçe", link.Value);
    }

    [Fact]
    public void Constructor_ShouldHaveEmptySubLinks_WhenNotProvided()
    {
        var link = Link.CreateNew("Fenerbahçe");
        Assert.Empty(link.SubLinkIds);
    }

    [Fact]
    public void Constructor_ShouldSetSubLinks()
    {
        var child = Link.CreateNew("Türkiye Milli Ligi");
        var link  = Link.CreateNew("Fenerbahçe", new[] { child.Id });
        Assert.Single(link.SubLinkIds);
        Assert.Equal(child.Id, link.SubLinkIds[0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateNew_ShouldThrow_WhenValueIsEmpty(string? value)
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => Link.CreateNew(value!));
        Assert.IsType<LinkCannotBeEmptyRule>(exception.BrokenRule);
    }

    [Fact]
    public void Id_ShouldBeDifferent_ForNewLinksWithSameValue()
    {
        // Entity: Her nesnenin kendine ait bir kimliği (ID) vardır.
        var a = Link.CreateNew("Real Madrid");
        var b = Link.CreateNew("Real Madrid");
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Equals_ShouldBeTrue_WhenSameInstance()
    {
        var a = Link.CreateNew("Real Madrid");
        var b = a;
        Assert.Equal(a, b);
    }
}
