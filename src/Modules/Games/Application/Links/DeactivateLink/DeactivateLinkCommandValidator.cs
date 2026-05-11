using FluentValidation;

namespace LexiLink.Modules.Games.Application.Links.DeactivateLink;

internal class DeactivateLinkCommandValidator : AbstractValidator<DeactivateLinkCommand>
{
    public DeactivateLinkCommandValidator()
    {
        RuleFor(x => x.LinkId).NotEmpty();
    }
}
