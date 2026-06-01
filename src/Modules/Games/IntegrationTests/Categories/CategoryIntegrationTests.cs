using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using LexiLink.Modules.Games.Application.Categories.EditCategory;
using LexiLink.Modules.Games.Application.Categories.GetCategories;
using LexiLink.Modules.Games.Application.Categories.GetCategoryDetails;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Categories;

[TestFixture]
public class CategoryIntegrationTests : TestBase
{
    [Test]
    public async Task CreateCategory_Test()
    {
        var categoryId = await ExecuteCommandAsync(
            new CreateCategoryCommand("Animals", "Animal-themed words", "en-US"));

        categoryId.Should().NotBe(Guid.Empty);

        var details = await ExecuteQueryAsync(new GetCategoryDetailsQuery(categoryId));
        details.Name.Should().Be("Animals");
        details.Description.Should().Be("Animal-themed words");
        details.Language.Should().Be("en-US");
        details.LinkCount.Should().Be(0);
    }

    [Test]
    public async Task EditCategory_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);

        await ExecuteCommandAsync(new EditCategoryCommand(categoryId, "Plants", "Plant-themed words", "en-US"));

        var details = await ExecuteQueryAsync(new GetCategoryDetailsQuery(categoryId));
        details.Name.Should().Be("Plants");
        details.Description.Should().Be("Plant-themed words");
        details.Language.Should().Be("en-US");
    }

    [Test]
    public async Task GetCategories_Test()
    {
        await CategoryHelper.CreateCategoryAsync(Sender, "Animals", language: "en-US");
        await CategoryHelper.CreateCategoryAsync(Sender, "Plants", language: "tr-TR");

        var categories = await ExecuteQueryAsync(new GetCategoriesQuery());

        categories.Should().HaveCount(2);
        categories.Select(c => c.Name).Should().Contain(["Animals", "Plants"]);
    }

    [Test]
    public async Task GetCategories_WhenLocaleProvided_FiltersByLanguage_Test()
    {
        await CategoryHelper.CreateCategoryAsync(Sender, "Animals", language: "en-US");
        await CategoryHelper.CreateCategoryAsync(Sender, "Spor", language: "tr-TR");

        var categories = await ExecuteQueryAsync(new GetCategoriesQuery("en-US"));

        categories.Should().ContainSingle();
        categories.Single().Name.Should().Be("Animals");
        categories.Single().Language.Should().Be("en-US");
    }

    [Test]
    public async Task GetCategoryDetails_LinkCountReflectsAssociatedLinks_Test()
    {
        var categoryId = await CategoryHelper.CreateCategoryAsync(Sender);
        await Sender.Send(new LexiLink.Modules.Games.Application.Links.CreateLink.CreateLinkCommand(
            categoryId, "cat", "", true));
        await Sender.Send(new LexiLink.Modules.Games.Application.Links.CreateLink.CreateLinkCommand(
            categoryId, "mat", "", true));

        var details = await ExecuteQueryAsync(new GetCategoryDetailsQuery(categoryId));

        details.LinkCount.Should().Be(2);
    }
}
