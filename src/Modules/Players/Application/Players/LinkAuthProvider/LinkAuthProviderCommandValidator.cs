using FluentValidation;

namespace LexiLink.Modules.Players.Application.Players.LinkAuthProvider;

internal class LinkAuthProviderCommandValidator : AbstractValidator<LinkAuthProviderCommand>
{
    public LinkAuthProviderCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.Provider).IsInEnum();
        RuleFor(x => x.ExternalId).NotEmpty();
    }
}
