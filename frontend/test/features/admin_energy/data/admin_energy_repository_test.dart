import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_energy/data/admin_energy_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminEnergyRepository _repo(MockClient client) => AdminEnergyRepository(
  apiClient: ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: client,
  ),
);

const _snapshotJson =
    '{'
    '"playerId":"00000000-0000-0000-0000-000000000abc",'
    '"currentAmount":3,"maximumAmount":5,"isFull":false,'
    '"rechargeIntervalSeconds":600,'
    '"lastRefilledOn":"2026-05-22T09:00:00Z",'
    '"secondsUntilNextRefill":300,'
    '"fullyRefilledAt":"2026-05-22T11:00:00Z"'
    '}';

void main() {
  group('AdminEnergyRepository', () {
    test('fetchSnapshot decodes payload', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'GET');
          expect(
            req.url.path,
            '/admin/players/00000000-0000-0000-0000-000000000abc/energy',
          );
          return http.Response(_snapshotJson, 200);
        }),
      );

      final snapshot = await repo.fetchSnapshot(
        '00000000-0000-0000-0000-000000000abc',
      );

      expect(snapshot.currentAmount, 3);
      expect(snapshot.maximumAmount, 5);
      expect(snapshot.isFull, isFalse);
      expect(snapshot.secondsUntilNextRefill, 300);
    });

    test('fetchSnapshot propagates 404 as ApiException', () async {
      final repo = _repo(
        MockClient((_) async => http.Response('{"detail":"nope"}', 404)),
      );

      expect(
        () => repo.fetchSnapshot('00000000-0000-0000-0000-000000000abc'),
        throwsA(isA<ApiException>()),
      );
    });

    test('setAmount posts to /set with amount body', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'POST');
          expect(
            req.url.path,
            '/admin/players/00000000-0000-0000-0000-000000000abc/energy/set',
          );
          expect(req.body, contains('"amount":2'));
          return http.Response('', 204);
        }),
      );

      await repo.setAmount(
        playerId: '00000000-0000-0000-0000-000000000abc',
        amount: 2,
      );
    });

    test('grant posts to /grant with amount body', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'POST');
          expect(
            req.url.path,
            '/admin/players/00000000-0000-0000-0000-000000000abc/energy/grant',
          );
          expect(req.body, contains('"amount":4'));
          return http.Response('', 204);
        }),
      );

      await repo.grant(
        playerId: '00000000-0000-0000-0000-000000000abc',
        amount: 4,
      );
    });

    test('reset posts to /reset', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'POST');
          expect(
            req.url.path,
            '/admin/players/00000000-0000-0000-0000-000000000abc/energy/reset',
          );
          return http.Response('', 204);
        }),
      );

      await repo.reset('00000000-0000-0000-0000-000000000abc');
    });
  });
}
