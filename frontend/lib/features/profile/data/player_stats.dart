import 'package:equatable/equatable.dart';

class PlayerStats extends Equatable {
  const PlayerStats({
    required this.playerId,
    required this.displayName,
    required this.discriminator,
    required this.handle,
    required this.avatarUrl,
    required this.locale,
    required this.isGuest,
    required this.authProvidersLinked,
    required this.gamesCompleted,
    required this.bestScore,
    required this.totalScore,
    required this.lastGameCompletedOn,
    required this.createdAt,
    required this.updatedAt,
  });

  factory PlayerStats.fromJson(Map<String, dynamic> json) {
    final playerId = json['playerId'];
    final displayName = json['displayName'];
    final discriminator = json['discriminator'];
    final handle = json['handle'];
    final avatarUrl = json['avatarUrl'];
    final locale = json['locale'];
    final isGuest = json['isGuest'];
    final authProvidersLinked = json['authProvidersLinked'];
    final gamesCompleted = json['gamesCompleted'];
    final bestScore = json['bestScore'];
    final totalScore = json['totalScore'];
    final lastGameCompletedOn = json['lastGameCompletedOn'];
    final createdAt = json['createdAt'];
    final updatedAt = json['updatedAt'];

    if (playerId is! String ||
        playerId.isEmpty ||
        (displayName != null && displayName is! String) ||
        (discriminator != null && discriminator is! int) ||
        (handle != null && handle is! String) ||
        (avatarUrl != null && avatarUrl is! String) ||
        (locale != null && locale is! String) ||
        isGuest is! bool ||
        authProvidersLinked is! int ||
        gamesCompleted is! int ||
        (bestScore != null && bestScore is! int) ||
        totalScore is! int ||
        (lastGameCompletedOn != null && lastGameCompletedOn is! String) ||
        createdAt is! String ||
        updatedAt is! String) {
      throw StateError('Player stats response is missing required fields.');
    }

    return PlayerStats(
      playerId: playerId,
      displayName: displayName as String?,
      discriminator: discriminator as int?,
      handle: handle as String?,
      avatarUrl: avatarUrl as String?,
      locale: locale as String?,
      isGuest: isGuest,
      authProvidersLinked: authProvidersLinked,
      gamesCompleted: gamesCompleted,
      bestScore: bestScore as int?,
      totalScore: totalScore,
      lastGameCompletedOn: lastGameCompletedOn == null
          ? null
          : DateTime.parse(lastGameCompletedOn as String),
      createdAt: DateTime.parse(createdAt),
      updatedAt: DateTime.parse(updatedAt),
    );
  }

  final String playerId;
  final String? displayName;
  final int? discriminator;
  final String? handle;
  final String? avatarUrl;
  final String? locale;
  final bool isGuest;
  final int authProvidersLinked;
  final int gamesCompleted;
  final int? bestScore;
  final int totalScore;
  final DateTime? lastGameCompletedOn;
  final DateTime createdAt;
  final DateTime updatedAt;

  @override
  List<Object?> get props => [
    playerId,
    displayName,
    discriminator,
    handle,
    avatarUrl,
    locale,
    isGuest,
    authProvidersLinked,
    gamesCompleted,
    bestScore,
    totalScore,
    lastGameCompletedOn,
    createdAt,
    updatedAt,
  ];
}
