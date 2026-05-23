import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_audit/data/admin_audit_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminAuditRepository _repo(MockClient client) => AdminAuditRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: client,
      ),
    );

const _twoActionsJson = '['
    '{"id":"00000000-0000-0000-0000-000000000001",'
    '"occurredOn":"2026-05-22T10:00:00Z",'
    '"adminUserId":"00000000-0000-0000-0000-aaaaaaaaaaaa",'
    '"actionType":"CreateCategoryCommand",'
    '"targetType":"Games.Category","targetId":null,'
    r'"payloadJson":"{\"name\":\"Spor\"}"},'
    '{"id":"00000000-0000-0000-0000-000000000002",'
    '"occurredOn":"2026-05-22T09:00:00Z",'
    '"adminUserId":"00000000-0000-0000-0000-aaaaaaaaaaaa",'
    '"actionType":"BanPlayerCommand",'
    '"targetType":"Players.Player",'
    '"targetId":"00000000-0000-0000-0000-bbbbbbbbbbbb",'
    r'"payloadJson":"{\"reason\":\"spam\"}"}'
    ']';

void main() {
  group('AdminAuditRepository', () {
    test('fetch decodes the list', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'GET');
        expect(req.url.path, '/admin/audit/');
        expect(req.url.queryParameters['offset'], '0');
        expect(req.url.queryParameters['limit'], '50');
        return http.Response(_twoActionsJson, 200);
      }));

      final actions = await repo.fetch();

      expect(actions, hasLength(2));
      expect(actions[0].actionType, 'CreateCategoryCommand');
      expect(actions[0].targetType, 'Games.Category');
      expect(actions[0].targetId, isNull);
      expect(actions[1].targetId,
          '00000000-0000-0000-0000-bbbbbbbbbbbb');
    });

    test('fetch passes optional filters and paging to query string',
        () async {
      final repo = _repo(MockClient((req) async {
        expect(req.url.queryParameters['adminUserId'],
            '00000000-0000-0000-0000-aaaaaaaaaaaa');
        expect(req.url.queryParameters['targetType'], 'Games.Category');
        expect(req.url.queryParameters['targetId'], 'abc');
        expect(req.url.queryParameters['offset'], '50');
        expect(req.url.queryParameters['limit'], '25');
        return http.Response('[]', 200);
      }));

      final actions = await repo.fetch(
        adminUserId: '00000000-0000-0000-0000-aaaaaaaaaaaa',
        targetType: 'Games.Category',
        targetId: 'abc',
        offset: 50,
        limit: 25,
      );

      expect(actions, isEmpty);
    });

    test('fetch omits filter keys when empty', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.url.queryParameters.containsKey('adminUserId'), isFalse);
        expect(req.url.queryParameters.containsKey('targetType'), isFalse);
        expect(req.url.queryParameters.containsKey('targetId'), isFalse);
        return http.Response('[]', 200);
      }));

      await repo.fetch(adminUserId: '', targetType: '', targetId: '');
    });
  });
}
