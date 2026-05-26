import 'package:lexilink_app/features/admin_undo/data/player_undo_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminUndoRepository {
  const AdminUndoRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerUndoSnapshot> fetchSnapshot(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId/undo');
    return PlayerUndoSnapshot.fromJson(raw);
  }

  Future<void> setBalance({
    required String playerId,
    required int balance,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/undo/set',
      body: {'balance': balance},
    );
  }

  /// Inventory is uncapped so grant never bounces on max.
  Future<void> grant({required String playerId, required int amount}) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/undo/grant',
      body: {'amount': amount},
    );
  }

  Future<void> reset(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/undo/reset');
  }
}
