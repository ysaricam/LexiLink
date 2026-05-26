namespace LexiLink.Modules.Reset.Application.PlayerResetInventories.GetPlayerReset;

public record PlayerResetSnapshotDto(
    Guid PlayerId,
    int Balance);
