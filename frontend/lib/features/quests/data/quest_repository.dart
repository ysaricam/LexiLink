import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class QuestRepository {
  const QuestRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<PlayerQuest>> getMe() async {
    final raw = await _apiClient.getJsonList('/quests/me');
    return raw
        .whereType<Map<String, dynamic>>()
        .map(PlayerQuest.fromJson)
        .toList(growable: false);
  }

  Future<void> claim(String playerQuestId) async {
    await _apiClient.postJson('/quests/$playerQuestId/claim');
  }
}
