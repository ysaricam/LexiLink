using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;

internal class RemoveOutgoingLinkCommandHandler : ICommandHandler<RemoveOutgoingLinkCommand>
{
    private readonly ILinkRepository _linkRepository;

    public RemoveOutgoingLinkCommandHandler(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public async Task Handle(RemoveOutgoingLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetByIdAsync(new LinkId(request.LinkId), cancellationToken)
            ?? throw new NotFoundException(nameof(Link), request.LinkId);

        link.RemoveOutgoingLink(new LinkId(request.OutgoingLinkId));
    }
}