using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Categories.Rules;

public sealed class GameCategoryNameCannotBeEmptyRule : IBusinessRule
{
    private readonly string _name;

    public GameCategoryNameCannotBeEmptyRule(string name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Oyun kategorisi adı boş veya sadece boşluktan oluşamaz.";
}