import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum SessionStatus {
  checking,
  unauthenticated,
  authenticated,
}

class SessionCubit extends Cubit<SessionState> {
  SessionCubit({
    required TokenStore tokenStore,
  }) : _tokenStore = tokenStore,
       super(const SessionState.checking());

  final TokenStore _tokenStore;

  Future<void> checkSession() async {
    final token = await _tokenStore.readAccessToken();
    if (token == null || token.isEmpty) {
      emit(const SessionState.unauthenticated());
      return;
    }

    emit(SessionState.authenticated(accessToken: token));
  }

  Future<void> setAuthenticated(
    String accessToken, {
    String? playerId,
  }) async {
    await _tokenStore.saveAccessToken(accessToken);
    if (playerId != null && playerId.isNotEmpty) {
      await _tokenStore.savePlayerId(playerId);
    }
    emit(SessionState.authenticated(accessToken: accessToken));
  }

  Future<void> signOut() async {
    await _tokenStore.clear();
    emit(const SessionState.unauthenticated());
  }
}

class SessionState extends Equatable {
  const SessionState({
    required this.status,
    this.accessToken,
  });

  const SessionState.checking() : this(status: SessionStatus.checking);

  const SessionState.unauthenticated()
    : this(status: SessionStatus.unauthenticated);

  const SessionState.authenticated({
    required String accessToken,
  }) : this(
         status: SessionStatus.authenticated,
         accessToken: accessToken,
       );

  final SessionStatus status;
  final String? accessToken;

  @override
  List<Object?> get props => [status, accessToken];
}
