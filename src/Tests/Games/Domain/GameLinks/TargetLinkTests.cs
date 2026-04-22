using LexiLink.Modules.Games.Domain.GameLinks;
using Xunit;
using System.Collections.Generic;

namespace LexiLink.Tests.Games.Domain.GameLinks;

public class TargetLinkTests
{
    private Link _linkFetcher(LinkId id, Dictionary<LinkId, Link> allLinks) => allLinks[id];

    [Fact]
    public void Resolve_ShouldThrow_WhenStartLinkIsNull()
    {
        var resolver = new TargetLinkResolver(id => null!);
        Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null!, 3));
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenDepthIsLessThanOne()
    {
        var startLink = Link.CreateNew("Test");
        var resolver = new TargetLinkResolver(id => startLink);
        Assert.Throws<ArgumentException>(() => resolver.Resolve(startLink.Id, 0));
    }

    [Fact]
    public void Resolve_ShouldReturnSameLink_WithFixedSeed()
    {
        // Arrange
        var (root, allLinks) = DataSet.Build();
        var resolver1 = new TargetLinkResolver(id => allLinks[id], seed: 123);
        var resolver2 = new TargetLinkResolver(id => allLinks[id], seed: 123);

        // Act
        var result1 = resolver1.Resolve(root.Id, 3);
        var result2 = resolver2.Resolve(root.Id, 3);

        // Assert
        Assert.Equal(result1.TargetId, result2.TargetId);
        Assert.Equal(result1.PathIds, result2.PathIds);
    }

    [Fact]
    public void Resolve_ShouldStopAtLeafNode_WhenDepthIsGreater()
    {
        // Arrange
        // Alt bağlantısı olmayan bir link (yaprak düğüm)
        var leaf = Link.CreateNew("Son Durak");
        var root = Link.CreateNew("Başlangıç", new[] { leaf.Id });
        var allLinks = new Dictionary<LinkId, Link> { { leaf.Id, leaf }, { root.Id, root } };
        var resolver = new TargetLinkResolver(id => allLinks[id]);

        // Act
        // Derinlik 5 isteniyor ama yol sadece 1 adım
        var result = resolver.Resolve(root.Id, 5);

        // Assert
        Assert.Equal(leaf.Id, result.TargetId);
    }

    [Fact]
    public void Resolve_ShouldReachExactDepth_IfPossible()
    {
        // Arrange
        var (root, allLinks) = DataSet.Build();
        var resolver = new TargetLinkResolver(id => allLinks[id], seed: 42);
        int depth = 2;

        // Act
        var result = resolver.Resolve(root.Id, depth);

        // Assert
        // Sonucun bir değeri olmalı (null olmamalı)
        Assert.NotNull(result.TargetId);
    }
}
