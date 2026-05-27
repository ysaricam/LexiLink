using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Categories.CreateCategory;

internal sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;

    internal CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var visibilityWindow = request.VisibilityStartsAt is null
            ? null
            : VisibilityWindow.Create(request.VisibilityStartsAt.Value, request.VisibilityEndsAt!.Value);

        var category = Category.Create(
            request.Name,
            request.SortOrder,
            request.Icon,
            visibilityWindow);

        await _categoryRepository.AddAsync(category, cancellationToken);
        return category.Id.Value;
    }
}
