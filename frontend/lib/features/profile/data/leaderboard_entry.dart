import 'package:equatable/equatable.dart';

class LeaderboardEntry extends Equatable {
  const LeaderboardEntry({
    required this.playerId,
    required this.displayName,
    required this.discriminator,
    required this.handle,
    required this.avatarUrl,
    required this.locale,
    required this.gamesCompleted,
    required this.bestScore,
    required this.totalScore,
    required this.lastGameCompletedOn,
  });

  factory LeaderboardEntry.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final displayName = json['displayName'];
    final discriminator = json['discriminator'];
    final handle = json['handle'];
    final avatarUrl = json['avatarUrl'];
    final locale = json['locale'];
    final gamesCompleted = json['gamesCompleted'];
    final bestScore = json['bestScore'];
    final totalScore = json['totalScore'];
    final lastGameCompletedOn = json['lastGameCompletedOn'];

    if (playerId is! String ||
        playerId.isEmpty ||
        (displayName != null && displayName is! String) ||
        (discriminator != null && discriminator is! int) ||
        (handle != null && handle is! String) ||
        (avatarUrl != null && avatarUrl is! String) ||
        (locale != null && locale is! String) ||
        gamesCompleted is! int ||
        (bestScore != null && bestScore is! int) ||
        totalScore is! int ||
        (lastGameCompletedOn != null && lastGameCompletedOn is! String)) {
      throw StateError(
        'Leaderboard entry response is missing required fields.',
      );
    }

    return LeaderboardEntry(
      playerId: playerId,
      displayName: displayName as String?,
      discriminator: discriminator as int?,
      handle: handle as String?,
      avatarUrl: avatarUrl as String?,
      locale: locale as String?,
      gamesCompleted: gamesCompleted,
      bestScore: bestScore as int?,
      totalScore: totalScore,
      lastGameCompletedOn: lastGameCompletedOn == null
          ? null
          : DateTime.parse(lastGameCompletedOn as String),
    );
  }

  final String playerId;
  final String? displayName;
  final int? discriminator;
  final String? handle;
  final String? avatarUrl;
  final String? locale;
  final int gamesCompleted;
  final int? bestScore;
  final int totalScore;
  final DateTime? lastGameCompletedOn;

  @override
  List<Object?> get props => [
    playerId,
    displayName,
    discriminator,
    handle,
    avatarUrl,
    locale,
    gamesCompleted,
    bestScore,
    totalScore,
    lastGameCompletedOn,
  ];
}
