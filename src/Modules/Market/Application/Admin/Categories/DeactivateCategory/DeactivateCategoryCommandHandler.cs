using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Categories.DeactivateCategory;

internal sealed class DeactivateCategoryCommandHandler : ICommandHandler<DeactivateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    internal DeactivateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            new CategoryId(request.CategoryId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        category.Deactivate();
    }
}
