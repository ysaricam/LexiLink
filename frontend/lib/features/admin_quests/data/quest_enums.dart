/// Server-side enums mirror. Wire format is the .NET enum name
/// (System.Text.Json JsonStringEnumConverter is registered on the
/// host). Post Sprint Q1 the closed AdminQuestType catalog is gone —
/// each quest definition carries free-text name + description and one
/// of three trigger types.
enum QuestTrigger {
  gameCompletedTotal('GameCompletedTotal'),
  gameCompletedDaily('GameCompletedDaily'),
  authProviderLinked('AuthProviderLinked');

  const QuestTrigger(this.wire);

  final String wire;

  String get displayLabel => switch (this) {
        QuestTrigger.gameCompletedTotal => 'Toplam oyun',
        QuestTrigger.gameCompletedDaily => 'Günlük oyun',
        QuestTrigger.authProviderLinked => 'Hesap bağlandı',
      };

  static QuestTrigger fromWire(String value) =>
      QuestTrigger.values.firstWhere(
        (t) => t.wire == value,
        orElse: () => throw FormatException('Unknown quest trigger: $value'),
      );
}

enum ProgressBaseline {
  fromSnapshot('FromSnapshot'),
  fromExistingTotal('FromExistingTotal');

  const ProgressBaseline(this.wire);

  final String wire;

  String get displayLabel => switch (this) {
        ProgressBaseline.fromSnapshot => 'Bu noktadan sonra',
        ProgressBaseline.fromExistingTotal => 'Tüm zamanlar',
      };

  static ProgressBaseline fromWire(String value) =>
      ProgressBaseline.values.firstWhere(
        (b) => b.wire == value,
        orElse: () => throw FormatException('Unknown progress baseline: $value'),
      );
}
