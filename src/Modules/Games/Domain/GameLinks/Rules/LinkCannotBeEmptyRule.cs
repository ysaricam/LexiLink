using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.GameLinks.Rules;

public sealed class LinkCannotBeEmptyRule : IBusinessRule
{
    private readonly string _value;

    public LinkCannotBeEmptyRule(string value)
    {
        _value = value;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_value);

    public string Message => "Bağlantı kelimesi boş veya sadece boşluktan oluşamaz.";
}
