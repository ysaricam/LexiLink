import 'package:lexilink_app/features/admin_auth/data/admin_session.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminAuthRepository {
  const AdminAuthRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  /// Exchanges an external admin identity (today: the
  /// `dev:admin:{email}` development verifier; production sso later)
  /// for a first-party admin JWT. The returned [AdminSession] is what
  /// callers persist via the admin token store and inject into subsequent
  /// admin API calls.
  Future<AdminSession> exchangeToken({
    required String email,
    required String externalToken,
  }) async {
    final response = await _apiClient.postJson(
      '/auth/admin/token',
      body: {'email': email, 'externalToken': externalToken},
    );

    final accessToken = response['accessToken'];
    final expiresAt = response['expiresAt'];
    final adminUserId = response['adminUserId'];
    final responseEmail = response['email'];
    final role = response['role'];

    if (accessToken is! String ||
        expiresAt is! String ||
        adminUserId is! String ||
        responseEmail is! String ||
        role is! String) {
      throw StateError(
        'Admin token exchange returned an unexpected payload shape.',
      );
    }

    return AdminSession(
      adminUserId: adminUserId,
      email: responseEmail,
      role: role,
      accessToken: accessToken,
      expiresAt: DateTime.parse(expiresAt),
    );
  }
}
