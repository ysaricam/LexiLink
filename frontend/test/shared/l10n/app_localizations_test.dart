import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/l10n/app_localizations.dart';
import 'package:lexilink_app/shared/l10n/l10n_extension.dart';

Future<AppLocalizations> _localizationsFor(
  WidgetTester tester,
  Locale locale,
) async {
  late AppLocalizations l10n;
  await tester.pumpWidget(
    MaterialApp(
      locale: locale,
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: Builder(
        builder: (context) {
          l10n = context.l10n;
          return const SizedBox.shrink();
        },
      ),
    ),
  );
  return l10n;
}

void main() {
  group('AppLocalizations', () {
    test('supports the five launch locales', () {
      final codes = AppLocalizations.supportedLocales
          .map((locale) => locale.languageCode)
          .toSet();
      expect(codes, containsAll(<String>{'tr', 'en', 'de', 'fr', 'es'}));
    });

    testWidgets('resolves a localized string for every supported locale', (
      tester,
    ) async {
      const expected = <String, String>{
        'en': 'Settings',
        'tr': 'Ayarlar',
        'de': 'Einstellungen',
        'fr': 'Paramètres',
        'es': 'Ajustes',
      };

      for (final entry in expected.entries) {
        final l10n = await _localizationsFor(tester, Locale(entry.key));
        expect(l10n.appTitle, 'WordLope');
        expect(
          l10n.settingsTitle,
          entry.value,
          reason: 'settingsTitle for ${entry.key}',
        );
      }
    });
  });
}
