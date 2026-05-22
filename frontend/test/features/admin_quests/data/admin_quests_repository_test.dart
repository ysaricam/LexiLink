import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_quests/data/admin_quests_repository.dart';
import 'package:lexilink_app/features/admin_quests/data/quest_enums.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminQuestsRepository _repo(MockClient client) => AdminQuestsRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: client,
      ),
    );

void main() {
  group('AdminQuestsRepository', () {
    test('fetchDefinitions decodes the list', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'GET');
        expect(req.url.path, '/admin/quests/definitions');
        return http.Response(
          '['
          '{"id":"00000000-0000-0000-0000-000000000001",'
          '"questType":"DailyThreeGames","cadence":"Daily",'
          '"goal":3,"rewardAmount":15,"prerequisiteQuestType":null,'
          '"isActive":true},'
          '{"id":"00000000-0000-0000-0000-000000000002",'
          '"questType":"ThreeGamesCompleted","cadence":"OneTime",'
          '"goal":3,"rewardAmount":10,'
          '"prerequisiteQuestType":"FirstGameCompleted",'
          '"isActive":false}'
          ']',
          200,
        );
      }));

      final defs = await repo.fetchDefinitions();

      expect(defs, hasLength(2));
      expect(defs[0].questType, AdminQuestType.dailyThreeGames);
      expect(defs[0].cadence, AdminQuestCadence.daily);
      expect(defs[0].goal, 3);
      expect(defs[0].isActive, isTrue);
      expect(defs[1].isActive, isFalse);
      expect(
        defs[1].prerequisiteQuestType,
        AdminQuestType.firstGameCompleted,
      );
    });

    test('createDefinition posts wire-form enums and returns id', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'POST');
        expect(req.url.path, '/admin/quests/definitions');
        expect(req.body, contains('"questType":"FirstGameCompleted"'));
        expect(req.body, contains('"cadence":"OneTime"'));
        expect(req.body, contains('"goal":1'));
        expect(req.body, contains('"rewardAmount":5'));
        return http.Response(
          '{"id":"00000000-0000-0000-0000-000000000099"}',
          201,
        );
      }));

      final id = await repo.createDefinition(
        questType: AdminQuestType.firstGameCompleted,
        cadence: AdminQuestCadence.oneTime,
        goal: 1,
        rewardAmount: 5,
      );

      expect(id, '00000000-0000-0000-0000-000000000099');
    });

    test('updateDefinition issues PUT with goal/reward/prereq', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'PUT');
        expect(
          req.url.path,
          '/admin/quests/definitions/00000000-0000-0000-0000-000000000005',
        );
        expect(req.body, contains('"goal":7'));
        expect(req.body, contains('"rewardAmount":42'));
        expect(req.body, contains('"prerequisiteQuestType":null'));
        return http.Response('', 204);
      }));

      await repo.updateDefinition(
        id: '00000000-0000-0000-0000-000000000005',
        goal: 7,
        rewardAmount: 42,
      );
    });

    test('deactivateDefinition posts to the deactivate sub-route', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'POST');
        expect(
          req.url.path,
          '/admin/quests/definitions/00000000-0000-0000-0000-000000000005/deactivate',
        );
        return http.Response('', 204);
      }));

      await repo.deactivateDefinition('00000000-0000-0000-0000-000000000005');
    });
  });
}
