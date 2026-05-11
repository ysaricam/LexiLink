using LexiLink.Modules.Games.Application.Links.ActivateLink;
using LexiLink.Modules.Games.Application.Links.AddOutgoingLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using LexiLink.Modules.Games.Application.Links.DeactivateLink;
using LexiLink.Modules.Games.Application.Links.GetLinkDetails;
using LexiLink.Modules.Games.Application.Links.GetLinkOutgoingLinks;
using LexiLink.Modules.Games.Application.Links.GetLinksByCategory;
using LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;
using LexiLink.Modules.Games.IntegrationTests.Categories;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Links;

[TestFixture]
public class LinkIntegrationTests : TestBase
{
    [Test]
    public async Task CreateLink_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);

        var linkId = await ExecuteCommandAsync(
            new CreateLinkCommand(categoryId, "cat", "feline", isActive: true));

        linkId.Should().NotBe(Guid.Empty);

        var details = await ExecuteQueryAsync(new GetLinkDetailsQuery(linkId));
        details.Value.Should().Be("cat");
        details.Description.Should().Be("feline");
        details.IsActive.Should().BeTrue();
        details.CategoryId.Should().Be(categoryId);
    }

    [Test]
    public async Task AddOutgoingLink_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var ids = await LinkHelper.CreateLinksAsync(Sender, categoryId, "cat", "mat");

        await ExecuteCommandAsync(new AddOutgoingLinkCommand(ids[0], ids[1]));

        var outgoing = await ExecuteQueryAsync(new GetLinkOutgoingLinksQuery(ids[0]));
        outgoing.Should().ContainSingle(l => l.Id == ids[1] && l.Value == "mat");
    }

    [Test]
    public async Task RemoveOutgoingLink_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var ids = await LinkHelper.CreateLinksAsync(Sender, categoryId, "cat", "mat");
        await ExecuteCommandAsync(new AddOutgoingLinkCommand(ids[0], ids[1]));

        await ExecuteCommandAsync(new RemoveOutgoingLinkCommand(ids[0], ids[1]));

        var outgoing = await ExecuteQueryAsync(new GetLinkOutgoingLinksQuery(ids[0]));
        outgoing.Should().BeEmpty();
    }

    [Test]
    public async Task DeactivateLink_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var linkId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "cat", isActive: true);

        await ExecuteCommandAsync(new DeactivateLinkCommand(linkId));

        var details = await ExecuteQueryAsync(new GetLinkDetailsQuery(linkId));
        details.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task ActivateLink_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        var linkId = await LinkHelper.CreateLinkAsync(Sender, categoryId, "cat", isActive: false);

        await ExecuteCommandAsync(new ActivateLinkCommand(linkId));

        var details = await ExecuteQueryAsync(new GetLinkDetailsQuery(linkId));
        details.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task GetLinksByCategory_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        await LinkHelper.CreateLinksAsync(Sender, categoryId, "cat", "mat", "bat");

        var links = await ExecuteQueryAsync(new GetLinksByCategoryQuery(categoryId));

        links.Should().HaveCount(3);
        links.Select(l => l.Value).Should().BeEquivalentTo("cat", "mat", "bat");
    }
}
