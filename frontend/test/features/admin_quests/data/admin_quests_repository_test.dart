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
          '"name":"Daily Three",'
          '"description":"Three games today",'
          '"trigger":"GameCompletedDaily",'
          '"threshold":3,"energyReward":15,"hintReward":0,"undoReward":0,"resetReward":0,'
          '"prerequisiteQuestDefinitionId":null,'
          '"progressBaseline":"FromSnapshot",'
          '"isActive":true},'
          '{"id":"00000000-0000-0000-0000-000000000002",'
          '"name":"Bronz",'
          '"description":"",'
          '"trigger":"GameCompletedTotal",'
          '"threshold":3,"energyReward":0,"hintReward":2,"undoReward":1,"resetReward":1,'
          '"prerequisiteQuestDefinitionId":"00000000-0000-0000-0000-000000000001",'
          '"progressBaseline":"FromExistingTotal",'
          '"isActive":false}'
          ']',
          200,
        );
      }));

      final defs = await repo.fetchDefinitions();

      expect(defs, hasLength(2));
      expect(defs[0].trigger, QuestTrigger.gameCompletedDaily);
      expect(defs[0].threshold, 3);
      expect(defs[0].isActive, isTrue);
      expect(defs[1].isActive, isFalse);
      expect(defs[1].progressBaseline, ProgressBaseline.fromExistingTotal);
      expect(
        defs[1].prerequisiteQuestDefinitionId,
        '00000000-0000-0000-0000-000000000001',
      );
    });

    test('createDefinition posts new shape and returns id', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'POST');
        expect(req.url.path, '/admin/quests/definitions');
        expect(req.body, contains('"name":"First Game"'));
        expect(req.body, contains('"trigger":"GameCompletedTotal"'));
        expect(req.body, contains('"threshold":1'));
        expect(req.body, contains('"energyReward":5'));
        expect(req.body, contains('"hintReward":2'));
        expect(req.body, contains('"undoReward":0'));
        expect(req.body, contains('"resetReward":0'));
        expect(req.body, contains('"progressBaseline":"FromSnapshot"'));
        return http.Response(
          '{"id":"00000000-0000-0000-0000-000000000099"}',
          201,
        );
      }));

      final id = await repo.createDefinition(
        name: 'First Game',
        description: 'Finish one game',
        trigger: QuestTrigger.gameCompletedTotal,
        threshold: 1,
        energyReward: 5,
        hintReward: 2,
        undoReward: 0,
        resetReward: 0,
        progressBaseline: ProgressBaseline.fromSnapshot,
      );

      expect(id, '00000000-0000-0000-0000-000000000099');
    });

    test('updateDefinition issues PUT with description/threshold/rewards', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'PUT');
        expect(
          req.url.path,
          '/admin/quests/definitions/00000000-0000-0000-0000-000000000005',
        );
        expect(req.body, contains('"description":"updated"'));
        expect(req.body, contains('"threshold":7'));
        expect(req.body, contains('"energyReward":42'));
        expect(req.body, contains('"hintReward":1'));
        expect(req.body, contains('"undoReward":0'));
        expect(req.body, contains('"resetReward":0'));
        expect(req.body, contains('"prerequisiteQuestDefinitionId":null'));
        expect(req.body, contains('"progressBaseline":"FromSnapshot"'));
        return http.Response('', 204);
      }));

      await repo.updateDefinition(
        id: '00000000-0000-0000-0000-000000000005',
        description: 'updated',
        threshold: 7,
        energyReward: 42,
        hintReward: 1,
        undoReward: 0,
        resetReward: 0,
        progressBaseline: ProgressBaseline.fromSnapshot,
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
