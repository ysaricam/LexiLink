using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using MediatR;

namespace LexiLink.Modules.Games.IntegrationTests.Categories;

internal static class CategoryHelper
{
    public static Task<Guid> CreateCategoryAsync(ISender sender, string name = "Animals", string description = "Animal-themed words")
        => sender.Send(new CreateCategoryCommand(name, description));
}
