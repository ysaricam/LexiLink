using FluentValidation;

namespace LexiLink.Modules.Games.Application.Links.CreateLink;

internal class CreateLinkCommandValidator : AbstractValidator<CreateLinkCommand>
{
    public CreateLinkCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
    }
}
