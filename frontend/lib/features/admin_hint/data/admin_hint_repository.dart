import 'package:lexilink_app/features/admin_hint/data/player_hint_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminHintRepository {
  const AdminHintRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerHintSnapshot> fetchSnapshot(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId/hint');
    return PlayerHintSnapshot.fromJson(raw);
  }

  Future<void> setBalance({
    required String playerId,
    required int balance,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/hint/set',
      body: {'balance': balance},
    );
  }

  /// Inventory is uncapped so grant never bounces on max.
  Future<void> grant({required String playerId, required int amount}) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/hint/grant',
      body: {'amount': amount},
    );
  }

  Future<void> reset(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/hint/reset');
  }
}
