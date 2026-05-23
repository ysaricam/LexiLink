import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/admin_players/application/admin_players_cubit.dart';
import 'package:lexilink_app/features/admin_players/data/admin_players_repository.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

class _Script {
  final List<http.Response Function(http.Request)> _steps = [];
  int _i = 0;

  void enqueue(http.Response Function(http.Request) responder) {
    _steps.add(responder);
  }

  http.Response respond(http.Request req) {
    if (_i >= _steps.length) {
      fail('Unexpected HTTP call: ${req.method} ${req.url.path}');
    }
    return _steps[_i++](req);
  }
}

AdminPlayersRepository _repoFromScript(_Script script) {
  return AdminPlayersRepository(
    apiClient: ApiClient(
      config: const ApiConfig(baseUrl: 'http://localhost:5000'),
      tokenStore: InMemoryTokenStore(),
      httpClient: MockClient((req) async => script.respond(req)),
    ),
  );
}

const _activePlayerJson = '{'
    '"id":"00000000-0000-0000-0000-000000000abc",'
    '"displayName":"Ada","discriminator":42,'
    '"handle":"ada","avatarUrl":null,'
    '"locale":"tr-TR","isGuest":false,'
    '"isBanned":false,'
    '"bannedReason":null,"bannedAt":null,'
    '"createdAt":"2026-01-01T00:00:00Z",'
    '"authProvidersLinked":1'
    '}';
const _bannedPlayerJson = '{'
    '"id":"00000000-0000-0000-0000-000000000abc",'
    '"displayName":"Ada","discriminator":42,'
    '"handle":"ada","avatarUrl":null,'
    '"locale":"tr-TR","isGuest":false,'
    '"isBanned":true,'
    '"bannedReason":"noisy",'
    '"bannedAt":"2026-05-22T10:00:00Z",'
    '"createdAt":"2026-01-01T00:00:00Z",'
    '"authProvidersLinked":1'
    '}';

void main() {
  group('AdminPlayersCubit', () {
    blocTest<AdminPlayersCubit, AdminPlayersState>(
      'lookup loads detail on success',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response(_activePlayerJson, 200));
        return AdminPlayersCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.lookup('00000000-0000-0000-0000-000000000abc'),
      verify: (cubit) {
        expect(cubit.state.status, AdminPlayersStatus.loaded);
        expect(cubit.state.detail?.handle, 'ada');
        expect(cubit.state.detail?.isBanned, isFalse);
      },
    );

    blocTest<AdminPlayersCubit, AdminPlayersState>(
      'lookup emits notFound on 404',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response('{"detail":"nope"}', 404));
        return AdminPlayersCubit(repository: _repoFromScript(script));
      },
      act: (cubit) => cubit.lookup('00000000-0000-0000-0000-000000000abc'),
      verify: (cubit) {
        expect(cubit.state.status, AdminPlayersStatus.notFound);
        expect(cubit.state.detail, isNull);
      },
    );

    blocTest<AdminPlayersCubit, AdminPlayersState>(
      'ban reloads detail with banned flag',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response(_activePlayerJson, 200))
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(_bannedPlayerJson, 200));
        return AdminPlayersCubit(repository: _repoFromScript(script));
      },
      act: (cubit) async {
        await cubit.lookup('00000000-0000-0000-0000-000000000abc');
        await cubit.ban(reason: 'noisy');
      },
      verify: (cubit) {
        expect(cubit.state.status, AdminPlayersStatus.loaded);
        expect(cubit.state.detail?.isBanned, isTrue);
        expect(cubit.state.detail?.bannedReason, 'noisy');
      },
    );

    blocTest<AdminPlayersCubit, AdminPlayersState>(
      'unban reloads detail with active flag',
      build: () {
        final script = _Script()
          ..enqueue((_) => http.Response(_bannedPlayerJson, 200))
          ..enqueue((_) => http.Response('', 204))
          ..enqueue((_) => http.Response(_activePlayerJson, 200));
        return AdminPlayersCubit(repository: _repoFromScript(script));
      },
      act: (cubit) async {
        await cubit.lookup('00000000-0000-0000-0000-000000000abc');
        await cubit.unban();
      },
      verify: (cubit) {
        expect(cubit.state.status, AdminPlayersStatus.loaded);
        expect(cubit.state.detail?.isBanned, isFalse);
      },
    );

    blocTest<AdminPlayersCubit, AdminPlayersState>(
      'ban without prior lookup is a no-op',
      build: () => AdminPlayersCubit(repository: _repoFromScript(_Script())),
      act: (cubit) => cubit.ban(reason: 'noisy'),
      verify: (cubit) {
        expect(cubit.state.status, AdminPlayersStatus.initial);
      },
    );
  });
}
