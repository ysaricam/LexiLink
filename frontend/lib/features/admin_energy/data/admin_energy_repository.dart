import 'package:lexilink_app/features/admin_energy/data/player_energy_snapshot.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminEnergyRepository {
  const AdminEnergyRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerEnergySnapshot> fetchSnapshot(String playerId) async {
    final raw = await _apiClient.getJson('/admin/players/$playerId/energy');
    return PlayerEnergySnapshot.fromJson(raw);
  }

  Future<void> setAmount({
    required String playerId,
    required int amount,
  }) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/energy/set',
      body: {'amount': amount},
    );
  }

  /// Permits over-max balance per Energy.PlayerEnergy.GrantBonus.
  Future<void> grant({required String playerId, required int amount}) async {
    await _apiClient.postJson(
      '/admin/players/$playerId/energy/grant',
      body: {'amount': amount},
    );
  }

  Future<void> reset(String playerId) async {
    await _apiClient.postJson('/admin/players/$playerId/energy/reset');
  }
}
