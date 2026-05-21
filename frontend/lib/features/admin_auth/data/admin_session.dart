import 'package:equatable/equatable.dart';

/// Snapshot of an authenticated admin principal. Mirrors the backend
/// `AdminTokenExchangeResponse` returned by `POST /auth/admin/token`
/// plus the access token used for subsequent admin API calls.
class AdminSession extends Equatable {
  const AdminSession({
    required this.adminUserId,
    required this.email,
    required this.role,
    required this.accessToken,
    required this.expiresAt,
  });

  final String adminUserId;
  final String email;
  final String role;
  final String accessToken;
  final DateTime expiresAt;

  @override
  List<Object?> get props => [adminUserId, email, role, accessToken, expiresAt];
}
