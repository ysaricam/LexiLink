import 'package:lexilink_app/features/game/data/game_details.dart';
import 'package:lexilink_app/features/game/data/hint_result.dart';
import 'package:lexilink_app/features/game/data/outgoing_link.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class GameRepository {
  const GameRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<String> createGame({
    required String playerId,
    required String categoryId,
    String difficulty = 'Easy',
  }) async {
    final response = await _apiClient.postJson(
      '/games',
      body: {
        'playerId': playerId,
        'categoryId': categoryId,
        'difficulty': difficulty,
      },
    );

    final id = response['id'];
    if (id is String && id.isNotEmpty) {
      return id;
    }

    throw StateError('Game creation did not return a game id.');
  }

  Future<void> startGame(String gameId) async {
    await _apiClient.postJson('/games/$gameId/start');
  }

  Future<GameDetails> getGame(String gameId) async {
    final response = await _apiClient.getJson('/games/$gameId');
    return GameDetails.fromJson(response);
  }

  Future<String?> getLinkDescription(String linkId) async {
    final response = await _apiClient.getJson('/links/$linkId');
    final description = response['description'];
    if (description is! String) return null;

    final trimmed = description.trim();
    return trimmed.isEmpty ? null : trimmed;
  }

  Future<List<OutgoingLink>> getOptions(String gameId) async {
    final response = await _apiClient.getJsonList('/games/$gameId/options');

    return response
        .map((item) {
          if (item is Map<String, dynamic>) {
            return OutgoingLink.fromJson(item);
          }

          throw StateError('Game options response contains an invalid item.');
        })
        .toList(growable: false);
  }

  Future<void> makeStep({
    required String gameId,
    required String nextLinkId,
  }) async {
    await _apiClient.postJson(
      '/games/$gameId/steps',
      body: {'nextLinkId': nextLinkId},
    );
  }

  Future<HintResult> useHint(String gameId) async {
    final response = await _apiClient.postJson('/games/$gameId/hint');
    return HintResult.fromJson(response);
  }

  Future<void> undo(String gameId) async {
    await _apiClient.postJson('/games/$gameId/undo');
  }

  Future<void> reset(String gameId) async {
    await _apiClient.postJson('/games/$gameId/reset');
  }

  Future<void> abandon(String gameId) async {
    await _apiClient.postJson('/games/$gameId/abandon');
  }
}
