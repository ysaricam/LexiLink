import 'package:lexilink_app/features/admin_diamond/data/player_diamond_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminDiamondRepository {
  const AdminDiamondRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerDiamondSnapshot> fetchSnapshot(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId/diamond');
    return PlayerDiamondSnapshot.fromJson(raw);
  }

  Future<void> setBalance({
    required String playerId,
    required int balance,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/diamond/set',
      body: {'balance': balance},
    );
  }

  Future<void> grant({
    required String playerId,
    required int amount,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/diamond/grant',
      body: {'amount': amount},
    );
  }

  Future<void> reset(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/diamond/reset');
  }
}
