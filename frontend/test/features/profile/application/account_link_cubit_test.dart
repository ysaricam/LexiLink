import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/features/auth/data/social_sign_in_service.dart';
import 'package:lexilink_app/features/profile/application/account_link_cubit.dart';
import 'package:lexilink_app/features/profile/data/account_link_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

const _appleIdentity = SocialIdentity(
  provider: SocialAuthProvider.apple,
  externalId: 'apple-user-1',
  externalToken: 'apple-token-1',
  email: 'apple@example.com',
);

void main() {
  group('AccountLinkCubit', () {
    late InMemoryTokenStore tokenStore;

    blocTest<AccountLinkCubit, AccountLinkState>(
      'links a new Apple account to the active guest session',
      setUp: () {
        tokenStore = InMemoryTokenStore();
      },
      build: () {
        return AccountLinkCubit(
          accountLinkRepository: _FakeAccountLinkRepository(
            session: const AppleContinueSession(
              playerId: 'guest-player-1',
              accessToken: 'jwt-guest-apple',
              mode: AppleContinueMode.linkedCurrentGuest,
            ),
          ),
          guestPlayerRepository: _FakeGuestPlayerRepository(),
          socialSignInService: const _FakeSocialSignInService(),
          tokenStore: tokenStore,
        );
      },
      act: (cubit) => cubit.linkApple(),
      expect: () => const [
        AccountLinkState(status: AccountLinkStatus.linkingApple),
        AccountLinkState.success(
          success: AccountLinkSuccess.linkedCurrentGuest,
        ),
      ],
      verify: (_) async {
        expect(await tokenStore.readAccessToken(), 'jwt-guest-apple');
        expect(await tokenStore.readPlayerId(), 'guest-player-1');
      },
    );

    blocTest<AccountLinkCubit, AccountLinkState>(
      'switches to an existing Apple player without deleting guest session',
      setUp: () {
        tokenStore = InMemoryTokenStore();
      },
      build: () {
        return AccountLinkCubit(
          accountLinkRepository: _FakeAccountLinkRepository(
            session: const AppleContinueSession(
              playerId: 'apple-player-1',
              accessToken: 'jwt-apple-player',
              mode: AppleContinueMode.switchedToExistingApplePlayer,
            ),
          ),
          guestPlayerRepository: _FakeGuestPlayerRepository(),
          socialSignInService: const _FakeSocialSignInService(),
          tokenStore: tokenStore,
        );
      },
      act: (cubit) => cubit.linkApple(),
      expect: () => const [
        AccountLinkState(status: AccountLinkStatus.linkingApple),
        AccountLinkState.success(
          success: AccountLinkSuccess.switchedToExistingApplePlayer,
        ),
      ],
      verify: (_) async {
        expect(await tokenStore.readAccessToken(), 'jwt-apple-player');
        expect(await tokenStore.readPlayerId(), 'apple-player-1');
      },
    );

    blocTest<AccountLinkCubit, AccountLinkState>(
      'surfaces Apple sign-in cancellation without changing token store',
      setUp: () {
        tokenStore = InMemoryTokenStore();
      },
      build: () {
        return AccountLinkCubit(
          accountLinkRepository: _FakeAccountLinkRepository(),
          guestPlayerRepository: _FakeGuestPlayerRepository(),
          socialSignInService: const _FakeSocialSignInService(
            error: SocialSignInException('Apple sign-in cancelled.'),
          ),
          tokenStore: tokenStore,
        );
      },
      act: (cubit) => cubit.linkApple(),
      expect: () => const [
        AccountLinkState(status: AccountLinkStatus.linkingApple),
        AccountLinkState.failure(message: 'Apple sign-in cancelled.'),
      ],
      verify: (_) async {
        expect(await tokenStore.readAccessToken(), isNull);
        expect(await tokenStore.readPlayerId(), isNull);
      },
    );

    blocTest<AccountLinkCubit, AccountLinkState>(
      'returns to the device guest session and stores its token',
      setUp: () {
        tokenStore = InMemoryTokenStore();
      },
      build: () {
        return AccountLinkCubit(
          accountLinkRepository: _FakeAccountLinkRepository(),
          guestPlayerRepository: _FakeGuestPlayerRepository(
            guestSession: const GuestSession(
              playerId: 'guest-player-1',
              accessToken: 'jwt-guest',
            ),
          ),
          socialSignInService: const _FakeSocialSignInService(),
          tokenStore: tokenStore,
        );
      },
      act: (cubit) => cubit.returnToGuest(
        deviceId: 'device-1',
        displayName: 'Guest Player',
        locale: 'en-US',
      ),
      expect: () => const [
        AccountLinkState(status: AccountLinkStatus.returningToGuest),
        AccountLinkState.success(success: AccountLinkSuccess.returnedToGuest),
      ],
      verify: (_) async {
        expect(await tokenStore.readAccessToken(), 'jwt-guest');
        expect(await tokenStore.readPlayerId(), 'guest-player-1');
      },
    );

    blocTest<AccountLinkCubit, AccountLinkState>(
      'does not overwrite session when Apple continue fails',
      setUp: () {
        tokenStore = InMemoryTokenStore();
      },
      build: () {
        return AccountLinkCubit(
          accountLinkRepository: _FakeAccountLinkRepository(
            error: const ApiException(
              statusCode: 401,
              message: 'Authentication is required.',
            ),
          ),
          guestPlayerRepository: _FakeGuestPlayerRepository(),
          socialSignInService: const _FakeSocialSignInService(),
          tokenStore: tokenStore,
        );
      },
      act: (cubit) => cubit.linkApple(),
      expect: () => const [
        AccountLinkState(status: AccountLinkStatus.linkingApple),
        AccountLinkState.failure(message: 'Authentication is required.'),
      ],
      verify: (_) async {
        expect(await tokenStore.readAccessToken(), isNull);
        expect(await tokenStore.readPlayerId(), isNull);
      },
    );
  });
}

class _FakeAccountLinkRepository implements AccountLinkRepository {
  _FakeAccountLinkRepository({this.session, this.error});

  final AppleContinueSession? session;
  final Exception? error;
  SocialIdentity? receivedIdentity;

  @override
  Future<AppleContinueSession> continueWithApple({
    required SocialIdentity identity,
  }) async {
    receivedIdentity = identity;
    final failure = error;
    if (failure != null) {
      throw failure;
    }
    return session!;
  }

  @override
  Future<void> linkProvider({
    required String playerId,
    required SocialIdentity identity,
  }) {
    throw UnimplementedError();
  }
}

class _FakeGuestPlayerRepository implements GuestPlayerRepository {
  _FakeGuestPlayerRepository({this.guestSession});

  final GuestSession? guestSession;

  @override
  Future<GuestSession> registerGuest({
    required String deviceId,
    required String displayName,
    required String locale,
  }) async {
    return guestSession!;
  }

  @override
  Future<GuestSession> exchangeSocialIdentity(SocialIdentity identity) {
    throw UnimplementedError();
  }
}

class _FakeSocialSignInService implements SocialSignInService {
  const _FakeSocialSignInService({this.error});

  final SocialSignInException? error;

  @override
  Future<SocialIdentity> signInWithApple() async {
    final failure = error;
    if (failure != null) {
      throw failure;
    }
    return _appleIdentity;
  }
}
