class PlayerAdminDetail {
  const PlayerAdminDetail({
    required this.id,
    required this.displayName,
    required this.discriminator,
    required this.handle,
    required this.locale,
    required this.isGuest,
    required this.isBanned,
    required this.createdAt,
    required this.authProvidersLinked,
    this.avatarUrl,
    this.bannedReason,
    this.bannedAt,
  });

  factory PlayerAdminDetail.fromJson(Map<String, dynamic> json) {
    return PlayerAdminDetail(
      id: json['id'] as String,
      displayName: json['displayName'] as String,
      discriminator: json['discriminator'] as int,
      handle: json['handle'] as String,
      avatarUrl: json['avatarUrl'] as String?,
      locale: json['locale'] as String,
      isGuest: json['isGuest'] as bool,
      isBanned: json['isBanned'] as bool,
      bannedReason: json['bannedReason'] as String?,
      bannedAt: json['bannedAt'] == null
          ? null
          : DateTime.parse(json['bannedAt'] as String),
      createdAt: DateTime.parse(json['createdAt'] as String),
      authProvidersLinked: json['authProvidersLinked'] as int,
    );
  }

  final String id;
  final String displayName;
  final int discriminator;
  final String handle;
  final String? avatarUrl;
  final String locale;
  final bool isGuest;
  final bool isBanned;
  final String? bannedReason;
  final DateTime? bannedAt;
  final DateTime createdAt;
  final int authProvidersLinked;
}
