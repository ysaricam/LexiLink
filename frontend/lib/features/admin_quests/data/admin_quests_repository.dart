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
    required String name,
    required String description,
    required QuestTrigger trigger,
    required int threshold,
    required int energyReward,
    required int hintReward,
    required ProgressBaseline progressBaseline,
    String? prerequisiteQuestDefinitionId,
  }) async {
    final response = await _apiClient.postJson(
      '/admin/quests/definitions',
      body: {
        'name': name,
        'description': description,
        'trigger': trigger.wire,
        'threshold': threshold,
        'energyReward': energyReward,
        'hintReward': hintReward,
        'prerequisiteQuestDefinitionId': prerequisiteQuestDefinitionId,
        'progressBaseline': progressBaseline.wire,
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
    required String description,
    required int threshold,
    required int energyReward,
    required int hintReward,
    required ProgressBaseline progressBaseline,
    String? prerequisiteQuestDefinitionId,
  }) async {
    await _apiClient.putJson(
      '/admin/quests/definitions/$id',
      body: {
        'description': description,
        'threshold': threshold,
        'energyReward': energyReward,
        'hintReward': hintReward,
        'prerequisiteQuestDefinitionId': prerequisiteQuestDefinitionId,
        'progressBaseline': progressBaseline.wire,
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
