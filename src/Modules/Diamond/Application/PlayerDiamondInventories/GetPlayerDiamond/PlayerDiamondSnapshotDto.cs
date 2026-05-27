namespace LexiLink.Modules.Diamond.Application.PlayerDiamondInventories.GetPlayerDiamond;

public record PlayerDiamondSnapshotDto(
    Guid PlayerId,
    int Balance);
