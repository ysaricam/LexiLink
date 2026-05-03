using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Application.Links.DeactivateLink;

internal class DeactivateLinkCommandHandler : ICommandHandler<DeactivateLinkCommand>
{
    private readonly ILinkRepository _linkRepository;

    internal DeactivateLinkCommandHandler(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public async Task Handle(DeactivateLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetByIdAsync(new LinkId(request.LinkId), cancellationToken)
            ?? throw new NotFoundException(nameof(Link), request.LinkId);

        link.Deactivate();
    }
}
