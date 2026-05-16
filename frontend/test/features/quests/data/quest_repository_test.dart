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
    "questType": "FirstGameCompleted",
    "state": "ReadyToClaim",
    "progress": 1,
    "goal": 1,
    "rewardAmount": 3,
    "issuedAt": "2026-05-15T09:00:00Z",
    "completedAt": "2026-05-15T09:05:00Z",
    "claimedAt": null,
    "expiresAt": null
  },
  {
    "id": "33333333-3333-3333-3333-333333333333",
    "playerId": "22222222-2222-2222-2222-222222222222",
    "questType": "DailyThreeGames",
    "state": "Active",
    "progress": 1,
    "goal": 3,
    "rewardAmount": 5,
    "issuedAt": "2026-05-15T09:00:00Z",
    "completedAt": null,
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
    expect(quests[0].questType, 'FirstGameCompleted');
    expect(quests[0].rewardAmount, 3);
    expect(quests[1].state, QuestState.active);
    expect(quests[1].progress, 1);
    expect(quests[1].goal, 3);
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
