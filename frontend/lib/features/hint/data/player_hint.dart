import 'package:equatable/equatable.dart';

class PlayerHint extends Equatable {
  const PlayerHint({
    required this.playerId,
    required this.balance,
  });

  factory PlayerHint.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final balance = json['balance'];

    if (playerId is! String || playerId.isEmpty || balance is! int) {
      throw StateError('Player hint response is missing required fields.');
    }

    return PlayerHint(playerId: playerId, balance: balance);
  }

  final String playerId;
  final int balance;

  @override
  List<Object?> get props => [playerId, balance];
}
