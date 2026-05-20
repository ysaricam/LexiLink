using FluentValidation;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;

internal sealed class CreateQuestDefinitionCommandValidator : AbstractValidator<CreateQuestDefinitionCommand>
{
    public CreateQuestDefinitionCommandValidator()
    {
        RuleFor(x => x.Goal).GreaterThan(0);
        RuleFor(x => x.RewardAmount).GreaterThan(0);
    }
}
