using FluentValidation;

namespace LexiLink.Modules.Games.Application.Links.RemoveOutgoingLink;

internal class RemoveOutgoingLinkCommandValidator : AbstractValidator<RemoveOutgoingLinkCommand>
{
    public RemoveOutgoingLinkCommandValidator()
    {
        RuleFor(x => x.LinkId).NotEmpty();
        RuleFor(x => x.OutgoingLinkId).NotEmpty();
    }
}
