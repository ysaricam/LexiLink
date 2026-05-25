import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

QuestRepository _build(MockClientHandler handler) {
  return QuestRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient(handler),
    ),
  );
}

void main() {
  test('parses a list of player quests', () async {
    final repository = _build((request) async {
      expect(request.url.path, '/quests/me');
      return http.Response(
        '''
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "playerId": "22222222-2222-2222-2222-222222222222",
    "questDefinitionId": "44444444-4444-4444-4444-444444444444",
    "name": "First game",
    "description": "Finish one game",
    "trigger": "GameCompletedTotal",
    "displayState": "ReadyToClaim",
    "progress": 1,
    "threshold": 1,
    "energyReward": 3,
    "hintReward": 0,
    "issuedAt": "2026-05-15T09:00:00Z",
    "claimedAt": null,
    "expiresAt": null
  },
  {
    "id": "33333333-3333-3333-3333-333333333333",
    "playerId": "22222222-2222-2222-2222-222222222222",
    "questDefinitionId": "55555555-5555-5555-5555-555555555555",
    "name": "Daily 3 games",
    "description": "Finish 3 games today",
    "trigger": "GameCompletedDaily",
    "displayState": "Active",
    "progress": 1,
    "threshold": 3,
    "energyReward": 5,
    "hintReward": 2,
    "issuedAt": "2026-05-15T09:00:00Z",
    "claimedAt": null,
    "expiresAt": "2026-05-16T00:00:00Z"
  }
]
''',
        200,
      );
    });

    final quests = await repository.getMe();

    expect(quests, hasLength(2));
    expect(quests[0].state, QuestState.readyToClaim);
    expect(quests[0].name, 'First game');
    expect(quests[0].energyReward, 3);
    expect(quests[0].hintReward, 0);
    expect(quests[1].state, QuestState.active);
    expect(quests[1].progress, 1);
    expect(quests[1].threshold, 3);
    expect(quests[1].energyReward, 5);
    expect(quests[1].hintReward, 2);
    expect(quests[1].expiresAt, DateTime.parse('2026-05-16T00:00:00Z'));
  });

  test('claim posts to the quest-specific path', () async {
    String? capturedPath;
    final repository = _build((request) async {
      capturedPath = request.url.path;
      expect(request.method, 'POST');
      return http.Response('', 204);
    });

    await repository.claim('11111111-1111-1111-1111-111111111111');

    expect(capturedPath, '/quests/11111111-1111-1111-1111-111111111111/claim');
  });
}
