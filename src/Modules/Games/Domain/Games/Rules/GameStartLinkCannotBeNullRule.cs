using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class GameStartLinkCannotBeNullRule : IBusinessRule
{
    private readonly LinkId? _startLinkId;

    public GameStartLinkCannotBeNullRule(LinkId? startLinkId)
    {
        _startLinkId = startLinkId;
    }

    public bool IsBroken() => _startLinkId is null;

    public string Message => "Oyun başlatılacak kelime kimliği (StartLinkId) boş olamaz.";
}
