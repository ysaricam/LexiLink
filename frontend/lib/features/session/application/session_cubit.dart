import 'dart:convert';

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

    if (_isExpiredJwt(token)) {
      await _tokenStore.clear();
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

  bool _isExpiredJwt(String token) {
    final parts = token.split('.');
    if (parts.length != 3) {
      return false;
    }

    try {
      final payload = utf8.decode(
        base64Url.decode(base64Url.normalize(parts[1])),
      );
      final json = jsonDecode(payload);
      if (json is! Map<String, dynamic>) {
        return false;
      }

      final exp = json['exp'];
      if (exp is! int) {
        return false;
      }

      final expiresAt = DateTime.fromMillisecondsSinceEpoch(
        exp * 1000,
        isUtc: true,
      );
      return !expiresAt.isAfter(DateTime.now().toUtc());
    } on Object {
      return false;
    }
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
