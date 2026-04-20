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
        var link = Link.Of("Fenerbahçe");
        Assert.Equal("Fenerbahçe", link.Value);
    }

    [Fact]
    public void Constructor_ShouldHaveEmptySubLinks_WhenNotProvided()
    {
        var link = Link.Of("Fenerbahçe");
        Assert.Empty(link.SubLinkIds);
    }

    [Fact]
    public void Constructor_ShouldSetSubLinks()
    {
        var child = Link.Of("Türkiye Milli Ligi");
        var link  = Link.Of("Fenerbahçe", new[] { child.Id });
        Assert.Single(link.SubLinkIds);
        Assert.Equal(child.Id, link.SubLinkIds[0]);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueIsEmpty()
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => Link.Of(""));
        Assert.IsType<LinkCannotBeEmptyRule>(exception.BrokenRule);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenValueIsWhitespace()
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => Link.Of("   "));
        Assert.IsType<LinkCannotBeEmptyRule>(exception.BrokenRule);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameReference()
    {
        var a = Link.Of("Real Madrid");
        var b = a;
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentIdEvenSameValue()
    {
        // Aggregate Root'lar ID bazlıdır. Aynı isimde olsa bile farklı nesnelerdir.
        var a = Link.Of("Real Madrid");
        var b = Link.Of("Real Madrid");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenDifferentValue()
    {
        var a = Link.Of("Real Madrid");
        var b = Link.Of("Barcelona");
        Assert.NotEqual(a, b);
    }
}
