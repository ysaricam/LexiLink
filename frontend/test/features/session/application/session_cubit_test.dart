import 'dart:convert';

import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/session/application/session_cubit.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

void main() {
  group('SessionCubit', () {
    blocTest<SessionCubit, SessionState>(
      'emits unauthenticated when no token exists',
      build: () => SessionCubit(tokenStore: InMemoryTokenStore()),
      act: (cubit) => cubit.checkSession(),
      expect: () => [const SessionState.unauthenticated()],
    );

    test('restores authenticated session when token already exists', () async {
      final tokenStore = InMemoryTokenStore();
      await tokenStore.saveAccessToken('token-1');
      final cubit = SessionCubit(tokenStore: tokenStore);

      await cubit.checkSession();

      expect(
        cubit.state,
        const SessionState.authenticated(accessToken: 'token-1'),
      );
    });

    test('restores authenticated session for an unexpired JWT', () async {
      final tokenStore = InMemoryTokenStore();
      final token = _jwtWithExpiration(
        DateTime.now().toUtc().add(const Duration(hours: 1)),
      );
      await tokenStore.saveAccessToken(token);
      final cubit = SessionCubit(tokenStore: tokenStore);

      await cubit.checkSession();

      expect(cubit.state, SessionState.authenticated(accessToken: token));
    });

    test('clears expired JWT and emits unauthenticated', () async {
      final tokenStore = InMemoryTokenStore();
      await tokenStore.saveAccessToken(
        _jwtWithExpiration(
          DateTime.now().toUtc().subtract(const Duration(minutes: 1)),
        ),
      );
      await tokenStore.savePlayerId('player-1');
      final cubit = SessionCubit(tokenStore: tokenStore);

      await cubit.checkSession();

      expect(cubit.state, const SessionState.unauthenticated());
      expect(await tokenStore.readAccessToken(), isNull);
      expect(await tokenStore.readPlayerId(), isNull);
    });

    blocTest<SessionCubit, SessionState>(
      'stores token and emits authenticated',
      build: () => SessionCubit(tokenStore: InMemoryTokenStore()),
      act: (cubit) => cubit.setAuthenticated('token-1'),
      expect: () => [
        const SessionState.authenticated(accessToken: 'token-1'),
      ],
    );

    blocTest<SessionCubit, SessionState>(
      'clears token and emits unauthenticated',
      build: () => SessionCubit(tokenStore: InMemoryTokenStore()),
      seed: () => const SessionState.authenticated(accessToken: 'token-1'),
      act: (cubit) => cubit.signOut(),
      expect: () => [const SessionState.unauthenticated()],
    );
  });
}

String _jwtWithExpiration(DateTime expiresAt) {
  String encode(Map<String, Object> json) {
    return base64UrlEncode(utf8.encode(jsonEncode(json))).replaceAll('=', '');
  }

  final exp = expiresAt.millisecondsSinceEpoch ~/ 1000;
  return '${encode({'alg': 'none'})}.${encode({'exp': exp})}.signature';
}
