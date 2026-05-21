import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_auth_repository.dart';
import 'package:lexilink_app/features/admin_auth/data/admin_session.dart';
import 'package:lexilink_app/shared/api/api_error.dart';
import 'package:lexilink_app/shared/storage/token_store.dart';

enum AdminSessionStatus {
  checking,
  unauthenticated,
  authenticating,
  authenticated,
  failure,
}

class AdminSessionCubit extends Cubit<AdminSessionState> {
  AdminSessionCubit({
    required AdminAuthRepository repository,
    required TokenStore adminTokenStore,
  }) : _repository = repository,
       _adminTokenStore = adminTokenStore,
       super(const AdminSessionState.checking());

  final AdminAuthRepository _repository;
  final TokenStore _adminTokenStore;

  /// Restores a previously-persisted admin token if any. Called on
  /// admin shell mount. We do not re-verify the token's validity here;
  /// the next API call will fail with 401 and trigger a sign-out.
  Future<void> checkSession() async {
    final token = await _adminTokenStore.readAccessToken();
    if (token == null || token.isEmpty) {
      emit(const AdminSessionState.unauthenticated());
      return;
    }
    emit(AdminSessionState.authenticated(accessToken: token));
  }

  Future<void> signIn({
    required String email,
    required String externalToken,
  }) async {
    emit(const AdminSessionState.authenticating());
    try {
      final session = await _repository.exchangeToken(
        email: email,
        externalToken: externalToken,
      );
      await _adminTokenStore.saveAccessToken(session.accessToken);
      emit(AdminSessionState.authenticatedWithSession(session));
    } on ApiException catch (e) {
      emit(AdminSessionState.failure(message: _messageFor(e)));
    } on Exception catch (e) {
      emit(AdminSessionState.failure(message: e.toString()));
    }
  }

  Future<void> signOut() async {
    await _adminTokenStore.clear();
    emit(const AdminSessionState.unauthenticated());
  }

  static String _messageFor(ApiException e) {
    return switch (e.statusCode) {
      401 => 'External token rejected. Check email and dev token.',
      404 => 'No active admin user is registered for this email.',
      400 => 'Email or external token is missing.',
      _ => e.message,
    };
  }
}

class AdminSessionState extends Equatable {
  const AdminSessionState({
    required this.status,
    this.accessToken,
    this.session,
    this.errorMessage,
  });

  const AdminSessionState.checking() : this(status: AdminSessionStatus.checking);

  const AdminSessionState.unauthenticated()
    : this(status: AdminSessionStatus.unauthenticated);

  const AdminSessionState.authenticating()
    : this(status: AdminSessionStatus.authenticating);

  const AdminSessionState.authenticated({required String accessToken})
    : this(
        status: AdminSessionStatus.authenticated,
        accessToken: accessToken,
      );

  AdminSessionState.authenticatedWithSession(AdminSession session)
    : this(
        status: AdminSessionStatus.authenticated,
        accessToken: session.accessToken,
        session: session,
      );

  const AdminSessionState.failure({required String message})
    : this(status: AdminSessionStatus.failure, errorMessage: message);

  final AdminSessionStatus status;
  final String? accessToken;
  final AdminSession? session;
  final String? errorMessage;

  @override
  List<Object?> get props => [status, accessToken, session, errorMessage];
}
