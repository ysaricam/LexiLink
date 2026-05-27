class PlayerDiamondSnapshot {
  const PlayerDiamondSnapshot({
    required this.playerId,
    required this.balance,
  });

  factory PlayerDiamondSnapshot.fromJson(Map<String, dynamic> json) {
    return PlayerDiamondSnapshot(
      playerId: json['playerId'] as String,
      balance: json['balance'] as int,
    );
  }

  final String playerId;
  final int balance;
}
