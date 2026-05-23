import 'package:lexilink_app/features/admin_audit/data/admin_action.dart';
import 'package:lexilink_app/shared/api/api_client.dart';

class AdminAuditRepository {
  const AdminAuditRepository({required ApiClient apiClient})
    : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<AdminAction>> fetch({
    String? adminUserId,
    String? targetType,
    String? targetId,
    int offset = 0,
    int limit = 50,
  }) async {
    final query = <String, String>{
      'offset': offset.toString(),
      'limit': limit.toString(),
      if (adminUserId != null && adminUserId.isNotEmpty)
        'adminUserId': adminUserId,
      if (targetType != null && targetType.isNotEmpty) 'targetType': targetType,
      if (targetId != null && targetId.isNotEmpty) 'targetId': targetId,
    };
    final raw = await _apiClient.getJsonList(
      '/admin/audit/',
      queryParameters: query,
    );
    return raw
        .cast<Map<String, dynamic>>()
        .map(AdminAction.fromJson)
        .toList(growable: false);
  }
}
