using LexiLink.Modules.Games.Domain.Links.Events;
using LexiLink.Modules.Games.Domain.Links.Rules;

namespace LexiLink.Modules.Games.Tests.Links;

[TestFixture]
public class LinkLifecycleTests : LinkTestsBase
{
    [Test]
    public void Activate_WhenInactive_RaisesLinkActivatedDomainEvent()
    {
        var link = CreateLink(isActive: false);

        link.Activate();

        AssertPublishedDomainEvent<LinkActivatedDomainEvent>(link)
            .LinkId.Should().Be(link.Id);
    }

    [Test]
    public void Activate_WhenAlreadyActive_BreaksLinkMustBeInactiveToActivateRule()
    {
        var link = CreateLink(isActive: true);

        AssertBrokenRule<LinkMustBeInactiveToActivateRule>(link.Activate);
    }

    [Test]
    public void Deactivate_WhenActive_RaisesLinkDeactivatedDomainEvent()
    {
        var link = CreateLink(isActive: true);

        link.Deactivate();

        AssertPublishedDomainEvent<LinkDeactivatedDomainEvent>(link)
            .LinkId.Should().Be(link.Id);
    }

    [Test]
    public void Deactivate_WhenAlreadyInactive_BreaksLinkMustBeActiveToDeactivateRule()
    {
        var link = CreateLink(isActive: false);

        AssertBrokenRule<LinkMustBeActiveToDeactivateRule>(link.Deactivate);
    }
}
