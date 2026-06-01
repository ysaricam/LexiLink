using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Application.Categories.EditCategory;

internal class EditCategoryCommandHandler : ICommandHandler<EditCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    internal EditCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(EditCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        category.EditGeneralInfo(request.Name, request.Description, request.Language);
    }
}
