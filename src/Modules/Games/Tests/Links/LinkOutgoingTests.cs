using LexiLink.Modules.Games.Domain.Links.Events;
using LexiLink.Modules.Games.Domain.Links.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Links;

[TestFixture]
public class LinkOutgoingTests : LinkTestsBase
{
    [Test]
    public void AddOutgoingLink_WhenValid_AddsAndRaisesEvent()
    {
        var categoryId = NewCategoryId();
        var source = CreateLink(categoryId);
        var targetId = NewLinkId();

        source.AddOutgoingLink(targetId, categoryId);

        source.OutgoingLinkIds.Should().ContainSingle(id => id == targetId);
        AssertPublishedDomainEvent<OutgoingLinkAddedDomainEvent>(source)
            .OutgoingLinkId.Should().Be(targetId);
    }

    [Test]
    public void AddOutgoingLink_WhenPointingToSelf_BreaksLinkCannotPointToItselfRule()
    {
        var categoryId = NewCategoryId();
        var link = CreateLink(categoryId);

        AssertBrokenRule<LinkCannotPointToItselfRule>(
            () => link.AddOutgoingLink(link.Id, categoryId));
    }

    [Test]
    public void AddOutgoingLink_WhenTargetIsDifferentCategory_BreaksLinkOutgoingMustBeSameCategoryRule()
    {
        var sourceCategoryId = NewCategoryId();
        var otherCategoryId = NewCategoryId();
        var source = CreateLink(sourceCategoryId);

        AssertBrokenRule<LinkOutgoingMustBeSameCategoryRule>(
            () => source.AddOutgoingLink(NewLinkId(), otherCategoryId));
    }

    [Test]
    public void AddOutgoingLink_WhenAlreadyExists_BreaksLinkOutgoingAlreadyExistsRule()
    {
        var categoryId = NewCategoryId();
        var source = CreateLink(categoryId);
        var targetId = NewLinkId();
        source.AddOutgoingLink(targetId, categoryId);

        AssertBrokenRule<LinkOutgoingAlreadyExistsRule>(
            () => source.AddOutgoingLink(targetId, categoryId));
    }

    [Test]
    public void RemoveOutgoingLink_WhenExists_RemovesAndRaisesEvent()
    {
        var categoryId = NewCategoryId();
        var source = CreateLink(categoryId);
        var targetId = NewLinkId();
        source.AddOutgoingLink(targetId, categoryId);
        DomainEventsTestHelper.ClearAllDomainEvents(source);

        source.RemoveOutgoingLink(targetId);

        source.OutgoingLinkIds.Should().BeEmpty();
        AssertPublishedDomainEvent<OutgoingLinkRemovedDomainEvent>(source)
            .OutgoingLinkId.Should().Be(targetId);
    }

    [Test]
    public void RemoveOutgoingLink_WhenNotExists_BreaksLinkOutgoingMustExistRule()
    {
        var source = CreateLink();

        AssertBrokenRule<LinkOutgoingMustExistRule>(
            () => source.RemoveOutgoingLink(NewLinkId()));
    }
}
