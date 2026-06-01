using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Categories.Events;
using LexiLink.Modules.Games.Domain.Categories.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Categories;

[TestFixture]
public class CategoryTests : TestBase
{
    private const string ValidName = "Animals";
    private const string ValidDescription = "Animal-themed words";
    private const string ValidLanguage = "en-US";

    [Test]
    public void Create_WithValidValues_IsSuccessful()
    {
        var category = Category.Create(ValidName, ValidDescription, ValidLanguage);

        category.Should().NotBeNull();
        category.Id.Should().NotBeNull();
        category.Language.Should().Be(ValidLanguage);
        AssertPublishedDomainEvent<CategoryCreatedDomainEvent>(category)
            .CategoryId.Should().Be(category.Id);
    }

    [Test]
    public void Create_WhenNameIsEmpty_BreaksCategoryNameMustNotBeEmptyRule()
    {
        AssertBrokenRule<CategoryNameMustNotBeEmptyRule>(() => Category.Create("", ValidDescription));
    }

    [Test]
    public void Create_WhenNameIsWhitespace_BreaksCategoryNameMustNotBeEmptyRule()
    {
        AssertBrokenRule<CategoryNameMustNotBeEmptyRule>(() => Category.Create("   ", ValidDescription));
    }

    [Test]
    public void Create_WhenNameExceedsMaxLength_BreaksCategoryNameMustNotExceedMaxLengthRule()
    {
        var tooLong = new string('a', CategoryNameMustNotExceedMaxLengthRule.MaxLength + 1);
        AssertBrokenRule<CategoryNameMustNotExceedMaxLengthRule>(() => Category.Create(tooLong, ValidDescription));
    }

    [Test]
    public void Create_WhenNameIsAtMaxLength_IsSuccessful()
    {
        var atMax = new string('a', CategoryNameMustNotExceedMaxLengthRule.MaxLength);
        var category = Category.Create(atMax, ValidDescription);
        category.Should().NotBeNull();
    }

    [Test]
    public void Create_WhenDescriptionExceedsMaxLength_BreaksCategoryDescriptionMustNotExceedMaxLengthRule()
    {
        var tooLong = new string('a', CategoryDescriptionMustNotExceedMaxLengthRule.MaxLength + 1);
        AssertBrokenRule<CategoryDescriptionMustNotExceedMaxLengthRule>(() => Category.Create(ValidName, tooLong));
    }

    [Test]
    public void Create_WhenLanguageFormatIsInvalid_BreaksCategoryLanguageMustBeValidFormatRule()
    {
        AssertBrokenRule<CategoryLanguageMustBeValidFormatRule>(() =>
            Category.Create(ValidName, ValidDescription, "english"));
    }

    [Test]
    public void EditGeneralInfo_WithValidValues_RaisesEditedEvent()
    {
        var category = Category.Create(ValidName, ValidDescription, ValidLanguage);

        category.EditGeneralInfo("Plants", "Plant-themed words", "tr-TR");

        category.Language.Should().Be("tr-TR");
        AssertPublishedDomainEvent<CategoryEditedDomainEvent>(category)
            .CategoryId.Should().Be(category.Id);
    }

    [Test]
    public void EditGeneralInfo_WhenNameIsEmpty_BreaksCategoryNameMustNotBeEmptyRule()
    {
        var category = Category.Create(ValidName, ValidDescription);
        AssertBrokenRule<CategoryNameMustNotBeEmptyRule>(() =>
            category.EditGeneralInfo("", ValidDescription, ValidLanguage));
    }

    [Test]
    public void EditGeneralInfo_WhenNameExceedsMaxLength_BreaksCategoryNameMustNotExceedMaxLengthRule()
    {
        var category = Category.Create(ValidName, ValidDescription);
        var tooLong = new string('a', CategoryNameMustNotExceedMaxLengthRule.MaxLength + 1);
        AssertBrokenRule<CategoryNameMustNotExceedMaxLengthRule>(() =>
            category.EditGeneralInfo(tooLong, ValidDescription, ValidLanguage));
    }

    [Test]
    public void EditGeneralInfo_WhenDescriptionExceedsMaxLength_BreaksCategoryDescriptionMustNotExceedMaxLengthRule()
    {
        var category = Category.Create(ValidName, ValidDescription);
        var tooLong = new string('a', CategoryDescriptionMustNotExceedMaxLengthRule.MaxLength + 1);
        AssertBrokenRule<CategoryDescriptionMustNotExceedMaxLengthRule>(() =>
            category.EditGeneralInfo(ValidName, tooLong, ValidLanguage));
    }

    [Test]
    public void EditGeneralInfo_WhenLanguageFormatIsInvalid_BreaksCategoryLanguageMustBeValidFormatRule()
    {
        var category = Category.Create(ValidName, ValidDescription, ValidLanguage);

        AssertBrokenRule<CategoryLanguageMustBeValidFormatRule>(() =>
            category.EditGeneralInfo(ValidName, ValidDescription, "en"));
    }
}
