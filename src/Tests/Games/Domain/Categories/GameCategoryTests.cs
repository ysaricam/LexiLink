using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Categories.Rules;
using Xunit;

namespace LexiLink.Tests.Games.Domain.Categories;

public class GameCategoryTests
{
    [Fact]
    public void CreateNew_ShouldInitializeCorrectly()
    {
        var category = GameCategory.CreateNew("Bilim", "Bilimsel terimler");

        Assert.Equal("Bilim", category.Name);
        Assert.Equal("Bilimsel terimler", category.Description);
        Assert.NotNull(category.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateNew_ShouldThrow_WhenNameIsEmpty(string? name)
    {
        var exception = Assert.Throws<BusinessRuleValidationException>(() => GameCategory.CreateNew(name!));
        Assert.IsType<GameCategoryNameCannotBeEmptyRule>(exception.BrokenRule);
    }
}