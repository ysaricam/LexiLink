import 'package:lexilink_app/features/admin_quests/data/quest_definition.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminQuestsRepository {
  const AdminQuestsRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<QuestDefinition>> fetchDefinitions() async {
    final raw = await _apiClient.getJsonList('/admin/quests/definitions');
    return raw
        .cast<Map<String, dynamic>>()
        .map(QuestDefinition.fromJson)
        .toList(growable: false);
  }

  Future<String> createDefinition({
    required AdminQuestType questType,
    required AdminQuestCadence cadence,
    required int goal,
    required int rewardAmount,
    AdminQuestType? prerequisiteQuestType,
  }) async {
    final response = await _apiClient.postJson(
      '/admin/quests/definitions',
      body: {
        'questType': questType.wire,
        'cadence': cadence.wire,
        'goal': goal,
        'rewardAmount': rewardAmount,
        'prerequisiteQuestType': prerequisiteQuestType?.wire,
      },
    );
    final id = response['id'];
    if (id is! String) {
      throw StateError(
        'Create quest definition returned an unexpected payload shape.',
      );
    }
    return id;
  }

  Future<void> updateDefinition({
    required String id,
    required int goal,
    required int rewardAmount,
    AdminQuestType? prerequisiteQuestType,
  }) async {
    await _apiClient.putJson(
      '/admin/quests/definitions/$id',
      body: {
        'goal': goal,
        'rewardAmount': rewardAmount,
        'prerequisiteQuestType': prerequisiteQuestType?.wire,
      },
    );
  }

  Future<void> deactivateDefinition(String id) async {
    await _apiClient.postJson('/admin/quests/definitions/$id/deactivate');
  }

  Future<void> reactivateDefinition(String id) async {
    await _apiClient.postJson('/admin/quests/definitions/$id/reactivate');
  }
}
