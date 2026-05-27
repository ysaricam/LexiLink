using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.Categories.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    internal UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            new CategoryId(request.CategoryId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var visibilityWindow = request.VisibilityStartsAt is null
            ? null
            : VisibilityWindow.Create(request.VisibilityStartsAt.Value, request.VisibilityEndsAt!.Value);

        category.Update(
            request.Name,
            request.SortOrder,
            request.Icon,
            visibilityWindow);
    }
}
