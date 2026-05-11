using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Application.Links.AddOutgoingLink;

internal class AddOutgoingLinkCommandHandler : ICommandHandler<AddOutgoingLinkCommand>
{
    private readonly ILinkRepository _linkRepository;

    public AddOutgoingLinkCommandHandler(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public async Task Handle(AddOutgoingLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await _linkRepository.GetByIdAsync(new LinkId(request.LinkId), cancellationToken)
            ?? throw new NotFoundException(nameof(Link), request.LinkId);

        var outgoingLink = await _linkRepository.GetByIdAsync(new LinkId(request.OutgoingLinkId), cancellationToken)
            ?? throw new NotFoundException(nameof(Link), request.OutgoingLinkId);

        link.AddOutgoingLink(outgoingLink.Id, outgoingLink.CategoryId);
    }
}