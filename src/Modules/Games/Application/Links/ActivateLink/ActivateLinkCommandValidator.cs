using FluentValidation;

namespace LexiLink.Modules.Games.Application.Links.ActivateLink;

internal class ActivateLinkCommandValidator : AbstractValidator<ActivateLinkCommand>
{
    public ActivateLinkCommandValidator()
    {
        RuleFor(x => x.LinkId).NotEmpty();
    }
}
