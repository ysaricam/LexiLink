import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/quests/application/quests_cubit.dart';
import 'package:lexilink_app/features/quests/data/player_quest.dart';
import 'package:lexilink_app/features/quests/data/quest_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _readyQuest = '''
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
}
''';

const _claimedQuest = '''
{
  "id": "11111111-1111-1111-1111-111111111111",
  "playerId": "22222222-2222-2222-2222-222222222222",
  "questDefinitionId": "44444444-4444-4444-4444-444444444444",
  "name": "First game",
  "description": "Finish one game",
  "trigger": "GameCompletedTotal",
  "displayState": "Claimed",
  "progress": 1,
  "threshold": 1,
  "energyReward": 3,
  "hintReward": 0,
  "issuedAt": "2026-05-15T09:00:00Z",
  "claimedAt": "2026-05-15T09:10:00Z",
  "expiresAt": null
}
''';

QuestsCubit _buildCubit({required MockClientHandler handler}) {
  final apiClient = ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: MockClient(handler),
  );

  return QuestsCubit(questRepository: QuestRepository(apiClient: apiClient));
}

void main() {
  group('QuestsCubit', () {
    blocTest<QuestsCubit, QuestsState>(
      'loads quests list',
      build: () => _buildCubit(
        handler: (request) async {
          expect(request.url.path, '/quests/me');
          return http.Response('[$_readyQuest]', 200);
        },
      ),
      act: (cubit) => cubit.loadQuests(),
      verify: (cubit) {
        expect(cubit.state.status, QuestsStatus.success);
        expect(cubit.state.quests, hasLength(1));
        expect(cubit.state.quests.first.state, QuestState.readyToClaim);
      },
    );

    blocTest<QuestsCubit, QuestsState>(
      'maps ApiException to failure message',
      build: () => _buildCubit(
        handler: (_) async => http.Response('', 401),
      ),
      act: (cubit) => cubit.loadQuests(),
      verify: (cubit) {
        expect(cubit.state.status, QuestsStatus.failure);
        expect(cubit.state.message, 'Authentication is required.');
      },
    );

    test('claim reloads quests and surfaces a reward message', () async {
      var call = 0;
      final cubit = _buildCubit(
        handler: (request) async {
          call++;
          if (request.method == 'GET') {
            // First reload: still ready (initial load).
            // Second reload (after claim): claimed.
            return http.Response(
              call == 1 ? '[$_readyQuest]' : '[$_claimedQuest]',
              200,
            );
          }
          expect(
            request.url.path,
            '/quests/11111111-1111-1111-1111-111111111111/claim',
          );
          return http.Response('', 204);
        },
      );

      await cubit.loadQuests();
      expect(cubit.state.quests.first.state, QuestState.readyToClaim);

      final ok = await cubit.claim(
        '11111111-1111-1111-1111-111111111111',
      );

      expect(ok, isTrue);
      expect(cubit.state.status, QuestsStatus.success);
      expect(cubit.state.quests.first.state, QuestState.claimed);
      expect(cubit.state.claimingId, isNull);
      expect(
        cubit.state.claimMessage,
        'Reward queued — your inventories will update in a few seconds.',
      );

      await cubit.close();
    });

    test('claim surfaces ApiException message and keeps quests', () async {
      final cubit = _buildCubit(
        handler: (request) async {
          if (request.method == 'GET') {
            return http.Response('[$_readyQuest]', 200);
          }
          return http.Response('', 404);
        },
      );

      await cubit.loadQuests();
      final ok = await cubit.claim(
        '11111111-1111-1111-1111-111111111111',
      );

      expect(ok, isFalse);
      expect(cubit.state.claimingId, isNull);
      expect(cubit.state.claimMessage, isNotNull);
      expect(cubit.state.quests.first.state, QuestState.readyToClaim);

      await cubit.close();
    });
  });
}
