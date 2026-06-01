using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using MediatR;

namespace LexiLink.Modules.Games.IntegrationTests.Categories;

internal static class CategoryHelper
{
    public static Task<Guid> CreateCategoryAsync(
        ISender sender,
        string name = "Animals",
        string description = "Animal-themed words",
        string language = "tr-TR")
        => sender.Send(new CreateCategoryCommand(name, description, language));
}
