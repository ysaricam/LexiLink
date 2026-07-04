import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:lexilink_app/features/auth/application/guest_entry_cubit.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/shared/api/api_client.dart';
import 'package:lexilink_app/shared/api/api_config.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

MockClient _guestFlowMockClient() => MockClient((request) async {
  if (request.url.path == '/players/guest') {
    return http.Response('{"id":"player-1"}', 201);
  }
  if (request.url.path == '/auth/token') {
    return http.Response(
      '{'
      '"accessToken":"jwt-player-1",'
      '"expiresAt":"2026-05-23T12:00:00Z",'
      '"playerId":"player-1"'
      '}',
      200,
    );
  }
  fail('Unexpected request: ${request.method} ${request.url.path}');
});

GuestEntryCubit _buildCubit({
  required TokenStore tokenStore,
  required SessionCubit sessionCubit,
  required MockClient httpClient,
}) {
  final apiClient = ApiClient(
    config: const ApiConfig(baseUrl: 'http://localhost:5000'),
    tokenStore: tokenStore,
    httpClient: httpClient,
  );

  return GuestEntryCubit(
    guestPlayerRepository: GuestPlayerRepository(apiClient: apiClient),
    sessionCubit: sessionCubit,
  );
}

void main() {
  group('GuestEntryCubit', () {
    blocTest<GuestEntryCubit, GuestEntryState>(
      'registers guest and authenticates session',
      build: () {
        final tokenStore = InMemoryTokenStore();
        final sessionCubit = SessionCubit(tokenStore: tokenStore);
        return _buildCubit(
          tokenStore: tokenStore,
          sessionCubit: sessionCubit,
          httpClient: _guestFlowMockClient(),
        );
      },
      act: (cubit) => cubit.continueAsGuest(
        deviceId: 'device-1',
        displayName: 'Guest Player',
        locale: 'en-US',
      ),
      expect: () => [
        const GuestEntryState.submitting(),
        const GuestEntryState.success(playerId: 'player-1'),
      ],
    );

    blocTest<GuestEntryCubit, GuestEntryState>(
      'resets guest entry state',
      build: () {
        final tokenStore = InMemoryTokenStore();
        final sessionCubit = SessionCubit(tokenStore: tokenStore);
        return _buildCubit(
          tokenStore: tokenStore,
          sessionCubit: sessionCubit,
          httpClient: _guestFlowMockClient(),
        );
      },
      seed: () => const GuestEntryState.success(playerId: 'player-1'),
      act: (cubit) => cubit.reset(),
      expect: () => [const GuestEntryState.idle()],
    );

    test('writes JWT access token to session token store', () async {
      final tokenStore = InMemoryTokenStore();
      final sessionCubit = SessionCubit(tokenStore: tokenStore);
      final cubit = _buildCubit(
        tokenStore: tokenStore,
        sessionCubit: sessionCubit,
        httpClient: _guestFlowMockClient(),
      );

      await cubit.continueAsGuest(
        deviceId: 'device-1',
        displayName: 'Guest Player',
        locale: 'en-US',
      );

      expect(tokenStore.accessToken, 'jwt-player-1');
      expect(sessionCubit.state.status, SessionStatus.authenticated);
    });
  });
}
