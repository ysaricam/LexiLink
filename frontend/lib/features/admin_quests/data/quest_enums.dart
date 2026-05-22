/// Server-side enums mirror. Wire format is the .NET enum name
/// (System.Text.Json JsonStringEnumConverter is registered on the
/// host).
enum AdminQuestType {
  firstGameCompleted('FirstGameCompleted'),
  threeGamesCompleted('ThreeGamesCompleted'),
  accountLinked('AccountLinked'),
  dailyThreeGames('DailyThreeGames');

  const AdminQuestType(this.wire);

  final String wire;

  static AdminQuestType fromWire(String value) =>
      AdminQuestType.values.firstWhere(
        (t) => t.wire == value,
        orElse: () => throw FormatException('Unknown quest type: $value'),
      );

  static AdminQuestType? tryFromWire(String? value) {
    if (value == null) return null;
    return AdminQuestType.fromWire(value);
  }
}

enum AdminQuestCadence {
  oneTime('OneTime'),
  daily('Daily');

  const AdminQuestCadence(this.wire);

  final String wire;

  static AdminQuestCadence fromWire(String value) =>
      AdminQuestCadence.values.firstWhere(
        (c) => c.wire == value,
        orElse: () => throw FormatException('Unknown quest cadence: $value'),
      );
}
