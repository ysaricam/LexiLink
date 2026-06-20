import 'package:lexilink_app/features/admin_players/data/player_admin_detail.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminPlayersRepository {
  const AdminPlayersRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  /// Looks up a player by handle. The caller is expected to handle the
  /// 404 surface (the server returns ProblemDetails).
  Future<PlayerAdminDetail> fetchDetailByHandle(String handle) async {
    final raw = await _apiClient.getJson(
      '/admin/players/by-handle',
      queryParameters: {'handle': handle},
    );
    return PlayerAdminDetail.fromJson(raw);
  }

  Future<PlayerAdminDetail> fetchDetailById(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId');
    return PlayerAdminDetail.fromJson(raw);
  }

  Future<void> ban({required String playerId, required String reason}) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/ban',
      body: {'reason': reason},
    );
  }

  Future<void> unban(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/unban');
  }
}
