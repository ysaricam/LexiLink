using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Application.Categories.CreateCategory;

internal class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;

    internal CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Description, request.Language);

        await _categoryRepository.AddAsync(category, cancellationToken);

        return category.Id.Value;
    }
}
