using LexiLink.Modules.Games.Domain.Links;

namespace LexiLink.Modules.Games.Domain.Games;

public record CompletedGameLinkPair(LinkId StartLinkId, LinkId TargetLinkId);
