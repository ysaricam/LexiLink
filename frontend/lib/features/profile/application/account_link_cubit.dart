import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/auth/data/social_identity.dart';
import 'package:lexilink_app/features/auth/data/social_sign_in_service.dart';
import 'package:lexilink_app/features/profile/data/account_link_repository.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum AccountLinkStatus {
  idle,
  linkingGoogle,
  linkingApple,
  success,
  failure,
}

class AccountLinkCubit extends Cubit<AccountLinkState> {
  AccountLinkCubit({
    required AccountLinkRepository accountLinkRepository,
    required SocialSignInService socialSignInService,
    required TokenStore tokenStore,
  }) : _accountLinkRepository = accountLinkRepository,
       _socialSignInService = socialSignInService,
       _tokenStore = tokenStore,
       super(const AccountLinkState.idle());

  final AccountLinkRepository _accountLinkRepository;
  final SocialSignInService _socialSignInService;
  final TokenStore _tokenStore;

  Future<void> linkGoogle() {
    return _link(
      status: AccountLinkStatus.linkingGoogle,
      signIn: _socialSignInService.signInWithGoogle,
    );
  }

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
      final playerId = await _tokenStore.readPlayerId();
      if (playerId == null || playerId.isEmpty) {
        emit(
          const AccountLinkState.failure(
            message: 'Player session is missing.',
          ),
        );
        return;
      }

      final identity = await signIn();
      await _accountLinkRepository.linkProvider(
        playerId: playerId,
        identity: identity,
      );
      emit(const AccountLinkState.success());
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
}

class AccountLinkState extends Equatable {
  const AccountLinkState({
    required this.status,
    this.message,
  });

  const AccountLinkState.idle() : this(status: AccountLinkStatus.idle);

  const AccountLinkState.success() : this(status: AccountLinkStatus.success);

  const AccountLinkState.failure({required String message})
    : this(status: AccountLinkStatus.failure, message: message);

  final AccountLinkStatus status;
  final String? message;

  bool get isBusy =>
      status == AccountLinkStatus.linkingGoogle ||
      status == AccountLinkStatus.linkingApple;

  @override
  List<Object?> get props => [status, message];
}
