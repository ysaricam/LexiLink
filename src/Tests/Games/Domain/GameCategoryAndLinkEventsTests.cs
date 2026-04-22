using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;
using LexiLink.Modules.Games.Domain.GameLinks.Events;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Categories.Events;
using Xunit;

namespace LexiLink.Tests.Games.Domain;

public class GameCategoryAndLinkEventsTests
{
    private T? GetPublishedEvent<T>(AggregateRoot aggregate) where T : class, IDomainEvent
    {
        return aggregate.DomainEvents?.OfType<T>().SingleOrDefault();
    }

    [Fact]
    public void Link_CreateNew_ShouldPublishLinkCreatedDomainEvent()
    {
        // Act
        var link = Link.CreateNew("Fenerbahçe");

        // Assert
        var domainEvent = GetPublishedEvent<LinkCreatedDomainEvent>(link);
        Assert.NotNull(domainEvent);
        Assert.Equal(link.Id, domainEvent!.LinkId);
        Assert.Equal("Fenerbahçe", domainEvent.Value);
    }

    [Fact]
    public void GameCategory_CreateNew_ShouldPublishGameCategoryCreatedDomainEvent()
    {
        // Act
        var category = GameCategory.CreateNew("Spor", "Spor ile ilgili kelimeler");

        // Assert
        var domainEvent = GetPublishedEvent<GameCategoryCreatedDomainEvent>(category);
        Assert.NotNull(domainEvent);
        Assert.Equal(category.Id, domainEvent!.CategoryId);
        Assert.Equal("Spor", domainEvent.Name);
    }
}