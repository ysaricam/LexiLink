import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_players/data/admin_players_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

AdminPlayersRepository _repo(MockClient client) => AdminPlayersRepository(
      apiClient: ApiClient(
        config: const ApiConfig(baseUrl: 'http://localhost:5000'),
        tokenStore: InMemoryTokenStore(),
        httpClient: client,
      ),
    );

void main() {
  group('AdminPlayersRepository', () {
    test('fetchDetail decodes payload', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'GET');
        expect(
          req.url.path,
          '/admin/players/00000000-0000-0000-0000-000000000abc',
        );
        return http.Response(
          '{'
          '"id":"00000000-0000-0000-0000-000000000abc",'
          '"displayName":"Ada","discriminator":42,'
          '"handle":"ada-42","avatarUrl":null,'
          '"locale":"tr-TR","isGuest":false,'
          '"isBanned":true,'
          '"bannedReason":"spam",'
          '"bannedAt":"2026-05-22T10:00:00Z",'
          '"createdAt":"2026-01-01T00:00:00Z",'
          '"authProvidersLinked":2'
          '}',
          200,
        );
      }));

      final detail = await repo
          .fetchDetail('00000000-0000-0000-0000-000000000abc');

      expect(detail.displayName, 'Ada');
      expect(detail.handle, 'ada-42');
      expect(detail.isBanned, isTrue);
      expect(detail.bannedReason, 'spam');
      expect(detail.bannedAt?.toUtc().toIso8601String(),
          '2026-05-22T10:00:00.000Z');
      expect(detail.authProvidersLinked, 2);
    });

    test('fetchDetail propagates 404 as ApiException', () async {
      final repo = _repo(
        MockClient((_) async => http.Response('{"detail":"nope"}', 404)),
      );

      expect(
        () => repo.fetchDetail('00000000-0000-0000-0000-000000000abc'),
        throwsA(isA<ApiException>()),
      );
    });

    test('ban posts to /ban with reason body', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'POST');
        expect(
          req.url.path,
          '/admin/players/00000000-0000-0000-0000-000000000abc/ban',
        );
        expect(req.body, contains('"reason":"too noisy"'));
        return http.Response('', 204);
      }));

      await repo.ban(
        playerId: '00000000-0000-0000-0000-000000000abc',
        reason: 'too noisy',
      );
    });

    test('unban posts to /unban', () async {
      final repo = _repo(MockClient((req) async {
        expect(req.method, 'POST');
        expect(
          req.url.path,
          '/admin/players/00000000-0000-0000-0000-000000000abc/unban',
        );
        return http.Response('', 204);
      }));

      await repo.unban('00000000-0000-0000-0000-000000000abc');
    });
  });
}
