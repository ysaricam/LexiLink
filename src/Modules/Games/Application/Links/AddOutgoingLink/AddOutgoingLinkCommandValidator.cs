using FluentValidation;

namespace LexiLink.Modules.Games.Application.Links.AddOutgoingLink;

internal class AddOutgoingLinkCommandValidator : AbstractValidator<AddOutgoingLinkCommand>
{
    public AddOutgoingLinkCommandValidator()
    {
        RuleFor(x => x.LinkId).NotEmpty();
        RuleFor(x => x.OutgoingLinkId).NotEmpty();
    }
}
