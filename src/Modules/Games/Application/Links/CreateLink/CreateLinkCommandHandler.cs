using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Application.Links.CreateLink;

internal class CreateLinkCommandHandler : ICommandHandler<CreateLinkCommand, Guid>
{
    private readonly ILinkRepository _linkRepository;
    private readonly ICategoryRepository _categoryRepository;

    internal CreateLinkCommandHandler(ILinkRepository linkRepository, ICategoryRepository categoryRepository)
    {
        _linkRepository = linkRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var link = Link.Create(
            category.Id,
            request.Value,
            request.Description,
            request.IsActive
        );

        await _linkRepository.AddAsync(link, cancellationToken);

        return link.Id.Value;
    }
}