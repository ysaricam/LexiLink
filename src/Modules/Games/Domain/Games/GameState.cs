namespace LexiLink.Modules.Games.Domain.Games;

public enum GameState
{
    Initial,
    InProgress,
    LastStepWarning,
    Completed,
    Failed,
    Abandoned
}