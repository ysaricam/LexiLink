class AdminAction {
  const AdminAction({
    required this.id,
    required this.occurredOn,
    required this.adminUserId,
    required this.actionType,
    required this.targetType,
    required this.payloadJson,
    this.targetId,
  });

  factory AdminAction.fromJson(Map<String, dynamic> json) {
    return AdminAction(
      id: json['id'] as String,
      occurredOn: DateTime.parse(json['occurredOn'] as String),
      adminUserId: json['adminUserId'] as String,
      actionType: json['actionType'] as String,
      targetType: json['targetType'] as String,
      targetId: json['targetId'] as String?,
      payloadJson: json['payloadJson'] as String,
    );
  }

  final String id;
  final DateTime occurredOn;
  final String adminUserId;
  final String actionType;
  final String targetType;
  final String? targetId;
  final String payloadJson;
}
