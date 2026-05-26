import 'package:lexilink_app/features/undo/data/player_undo.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class UndoRepository {
  const UndoRepository({required ApiClient apiClient}) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<PlayerUndo> getMe() async {
    final response = await _apiClient.getJson('/undo/me');
    return PlayerUndo.fromJson(response);
  }
}
