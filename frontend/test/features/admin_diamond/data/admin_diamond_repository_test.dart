import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_diamond/data/admin_diamond_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminDiamondRepository _repo(MockClient client) => AdminDiamondRepository(
  apiClient: ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: InMemoryTokenStore(),
    httpClient: client,
  ),
);

void main() {
  group('AdminDiamondRepository', () {
    test('fetchSnapshot calls diamond admin endpoint', () async {
      final repo = _repo(
        MockClient((req) async {
          expect(req.method, 'GET');
          expect(
            req.url.path,
            '/admin/players/00000000-0000-0000-0000-000000000abc/diamond',
          );
          return http.Response(
            '{"playerId":"00000000-0000-0000-0000-000000000abc","balance":7}',
            200,
          );
        }),
      );

      final snapshot = await repo.fetchSnapshot(
        '00000000-0000-0000-0000-000000000abc',
      );

      expect(snapshot.balance, 7);
    });

    test('set/grant/reset call expected endpoints', () async {
      final calls = <String>[];
      final repo = _repo(
        MockClient((req) async {
          calls.add('${req.method} ${req.url.path} ${req.body}');
          return http.Response('', 204);
        }),
      );

      const playerId = '00000000-0000-0000-0000-000000000abc';
      await repo.setBalance(playerId: playerId, balance: 11);
      await repo.grant(playerId: playerId, amount: 3);
      await repo.reset(playerId);

      expect(calls[0], contains('/admin/players/$playerId/diamond/set'));
      expect(calls[0], contains('"balance":11'));
      expect(calls[1], contains('/admin/players/$playerId/diamond/grant'));
      expect(calls[1], contains('"amount":3'));
      expect(calls[2], contains('/admin/players/$playerId/diamond/reset'));
    });
  });
}
