import 'package:equatable/equatable.dart';

class PlayerUndo extends Equatable {
  const PlayerUndo({
    required this.playerId,
    required this.balance,
  });

  factory PlayerUndo.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final balance = json['balance'];

    if (playerId is! String || playerId.isEmpty || balance is! int) {
      throw StateError('Player undo response is missing required fields.');
    }

    return PlayerUndo(playerId: playerId, balance: balance);
  }

  final String playerId;
  final int balance;

  @override
  List<Object?> get props => [playerId, balance];
}
