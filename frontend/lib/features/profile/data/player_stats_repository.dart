import 'package:lexilink_app/features/profile/data/leaderboard_entry.dart';
import 'package:lexilink_app/features/profile/data/leaderboard_query.dart';
import 'package:lexilink_app/features/profile/data/player_stats.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class PlayerStatsRepository {
  const PlayerStatsRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerStats> getPlayerStats(String playerId) async {
    final response = await _apiClient.getJson('/stats/players/$playerId');
    return PlayerStats.fromJson(response);
  }

  Future<void> updatePlayerProfile({
    required String playerId,
    required String? avatarUrl,
    required String locale,
    required String displayName,
    required int discriminator,
  }) async {
    await _apiClient.patchJson(
      '/players/$playerId/profile',
      body: {
        'avatarUrl': avatarUrl,
        'locale': locale,
        'displayName': displayName,
        'discriminator': discriminator,
      },
    );
  }

  Future<List<LeaderboardEntry>> getLeaderboard({
    LeaderboardQuery query = const LeaderboardQuery(),
  }) async {
    final response = await _apiClient.getJsonList(
      '/stats/leaderboard',
      queryParameters: query.toQueryParameters(),
    );

    return response
        .map((item) {
          if (item is Map<String, dynamic>) {
            return LeaderboardEntry.fromJson(item);
          }

          throw StateError('Leaderboard response contains an invalid item.');
        })
        .toList(growable: false);
  }
}
