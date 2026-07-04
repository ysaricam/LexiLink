import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/auth/data/guest_player_repository.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/features/auth/data/social_sign_in_service.dart';
import 'package:lexilink_app/features/profile/data/account_link_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum AccountLinkStatus {
  idle,
  linkingApple,
  returningToGuest,
  success,
  failure,
}

enum AccountLinkSuccess {
  linkedCurrentGuest,
  switchedToExistingApplePlayer,
  returnedToGuest,
}

class AccountLinkCubit extends Cubit<AccountLinkState> {
  AccountLinkCubit({
    required AccountLinkRepository accountLinkRepository,
    required GuestPlayerRepository guestPlayerRepository,
    required SocialSignInService socialSignInService,
    required TokenStore tokenStore,
  }) : _accountLinkRepository = accountLinkRepository,
       _guestPlayerRepository = guestPlayerRepository,
       _socialSignInService = socialSignInService,
       _tokenStore = tokenStore,
       super(const AccountLinkState.idle());

  final AccountLinkRepository _accountLinkRepository;
  final GuestPlayerRepository _guestPlayerRepository;
  final SocialSignInService _socialSignInService;
  final TokenStore _tokenStore;

  Future<void> linkApple() {
    return _link(
      status: AccountLinkStatus.linkingApple,
      signIn: _socialSignInService.signInWithApple,
    );
  }

  Future<void> _link({
    required AccountLinkStatus status,
    required Future<SocialIdentity> Function() signIn,
  }) async {
    emit(AccountLinkState(status: status));

    try {
      final identity = await signIn();
      final session = await _accountLinkRepository.continueWithApple(
        identity: identity,
      );
      await _saveSession(session.accessToken, session.playerId);
      emit(
        AccountLinkState.success(
          success: switch (session.mode) {
            AppleContinueMode.linkedCurrentGuest =>
              AccountLinkSuccess.linkedCurrentGuest,
            AppleContinueMode.switchedToExistingApplePlayer =>
              AccountLinkSuccess.switchedToExistingApplePlayer,
          },
        ),
      );
    } on SocialSignInException catch (error) {
      emit(AccountLinkState.failure(message: error.message));
    } on ApiException catch (error) {
      emit(AccountLinkState.failure(message: error.message));
    } on Exception {
      emit(
        const AccountLinkState.failure(
          message: 'Account link failed. Try again.',
        ),
      );
    }
  }

  Future<void> returnToGuest({
    required String deviceId,
    required String displayName,
    required String locale,
  }) async {
    emit(const AccountLinkState(status: AccountLinkStatus.returningToGuest));

    try {
      final session = await _guestPlayerRepository.registerGuest(
        deviceId: deviceId,
        displayName: displayName,
        locale: locale,
      );
      await _saveSession(session.accessToken, session.playerId);
      emit(
        const AccountLinkState.success(
          success: AccountLinkSuccess.returnedToGuest,
        ),
      );
    } on ApiException catch (error) {
      emit(AccountLinkState.failure(message: error.message));
    } on Exception {
      emit(
        const AccountLinkState.failure(
          message: 'Could not return to guest session. Try again.',
        ),
      );
    }
  }

  Future<void> _saveSession(String accessToken, String playerId) async {
    await _tokenStore.saveAccessToken(accessToken);
    await _tokenStore.savePlayerId(playerId);
  }
}

class AccountLinkState extends Equatable {
  const AccountLinkState({
    required this.status,
    this.success,
    this.message,
  });

  const AccountLinkState.idle() : this(status: AccountLinkStatus.idle);

  const AccountLinkState.success({required AccountLinkSuccess success})
    : this(status: AccountLinkStatus.success, success: success);

  const AccountLinkState.failure({required String message})
    : this(status: AccountLinkStatus.failure, message: message);

  final AccountLinkStatus status;
  final AccountLinkSuccess? success;
  final String? message;

  bool get isBusy =>
      status == AccountLinkStatus.linkingApple ||
      status == AccountLinkStatus.returningToGuest;

  @override
  List<Object?> get props => [status, success, message];
}
