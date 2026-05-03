using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Application.Links.ActivateLink;

internal class ActivateLinkCommandHandler : ICommandHandler<ActivateLinkCommand>
{
    private readonly ILinkRepository _linkRepository;

    internal ActivateLinkCommandHandler(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public async Task Handle(ActivateLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetByIdAsync(new LinkId(request.LinkId), cancellationToken)
            ?? throw new NotFoundException(nameof(Link), request.LinkId);

        link.Activate();
    }
}
