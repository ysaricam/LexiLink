import 'package:lexilink_app/features/admin_reset/data/player_reset_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminResetRepository {
  const AdminResetRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerResetSnapshot> fetchSnapshot(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId/reset');
    return PlayerResetSnapshot.fromJson(raw);
  }

  Future<void> setBalance({
    required String playerId,
    required int balance,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/reset/set',
      body: {'balance': balance},
    );
  }

  /// Inventory is uncapped so grant never bounces on max.
  Future<void> grant({required String playerId, required int amount}) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/reset/grant',
      body: {'amount': amount},
    );
  }

  Future<void> reset(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/reset/reset');
  }
}
