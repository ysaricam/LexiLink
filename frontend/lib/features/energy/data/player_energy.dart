import 'package:equatable/equatable.dart';

class PlayerEnergy extends Equatable {
  const PlayerEnergy({
    required this.playerId,
    required this.currentAmount,
    required this.maximumAmount,
    required this.isFull,
    required this.rechargeIntervalSeconds,
    required this.lastRefilledOn,
    required this.secondsUntilNextRefill,
    required this.fullyRefilledAt,
  });

  factory PlayerEnergy.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final currentAmount = json['currentAmount'];
    final maximumAmount = json['maximumAmount'];
    final isFull = json['isFull'];
    final rechargeIntervalSeconds = json['rechargeIntervalSeconds'];
    final lastRefilledOn = json['lastRefilledOn'];
    final secondsUntilNextRefill = json['secondsUntilNextRefill'];
    final fullyRefilledAt = json['fullyRefilledAt'];

    if (playerId is! String ||
        playerId.isEmpty ||
        currentAmount is! int ||
        maximumAmount is! int ||
        isFull is! bool ||
        rechargeIntervalSeconds is! int ||
        lastRefilledOn is! String ||
        (secondsUntilNextRefill != null && secondsUntilNextRefill is! int) ||
        (fullyRefilledAt != null && fullyRefilledAt is! String)) {
      throw StateError('Player energy response is missing required fields.');
    }

    return PlayerEnergy(
      playerId: playerId,
      currentAmount: currentAmount,
      maximumAmount: maximumAmount,
      isFull: isFull,
      rechargeIntervalSeconds: rechargeIntervalSeconds,
      lastRefilledOn: DateTime.parse(lastRefilledOn),
      secondsUntilNextRefill: secondsUntilNextRefill as int?,
      fullyRefilledAt: fullyRefilledAt == null
          ? null
          : DateTime.parse(fullyRefilledAt as String),
    );
  }

  final String playerId;
  final int currentAmount;
  final int maximumAmount;
  final bool isFull;
  final int rechargeIntervalSeconds;
  final DateTime lastRefilledOn;
  final int? secondsUntilNextRefill;
  final DateTime? fullyRefilledAt;

  @override
  List<Object?> get props => [
    playerId,
    currentAmount,
    maximumAmount,
    isFull,
    rechargeIntervalSeconds,
    lastRefilledOn,
    secondsUntilNextRefill,
    fullyRefilledAt,
  ];
}
