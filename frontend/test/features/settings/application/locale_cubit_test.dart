import 'package:bloc_test/bloc_test.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/features/settings/application/locale_cubit.dart';
import 'package:lexilink_app/features/settings/data/app_language.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/player_locale_writer.dart';

class _RecordingLocaleWriter implements PlayerLocaleWriter {
  final List<String> written = [];

  @override
  Future<void> updateLocale(String backendLocale) async {
    written.add(backendLocale);
  }
}

void main() {
  group('LocaleCubit.load', () {
    blocTest<LocaleCubit, AppLanguage>(
      'uses the stored language when present',
      build: () => LocaleCubit(
        repository: InMemoryLocalePreferencesRepository(AppLanguage.german),
        localeWriter: const NoopPlayerLocaleWriter(),
        deviceLanguageResolver: () => 'fr',
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) => expect(cubit.state, AppLanguage.german),
    );

    blocTest<LocaleCubit, AppLanguage>(
      'falls back to the device language when nothing is stored',
      build: () => LocaleCubit(
        repository: InMemoryLocalePreferencesRepository(),
        localeWriter: const NoopPlayerLocaleWriter(),
        deviceLanguageResolver: () => 'es',
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) => expect(cubit.state, AppLanguage.spanish),
    );

    blocTest<LocaleCubit, AppLanguage>(
      'falls back to English for an unsupported device language',
      build: () => LocaleCubit(
        repository: InMemoryLocalePreferencesRepository(),
        localeWriter: const NoopPlayerLocaleWriter(),
        deviceLanguageResolver: () => 'ja',
      ),
      act: (cubit) => cubit.load(),
      verify: (cubit) => expect(cubit.state, AppLanguage.english),
    );
  });

  group('LocaleCubit.setLanguage', () {
    blocTest<LocaleCubit, AppLanguage>(
      'persists and writes the region-qualified backend locale',
      build: () {
        final repository = InMemoryLocalePreferencesRepository();
        final writer = _RecordingLocaleWriter();
        return LocaleCubit(
          repository: repository,
          localeWriter: writer,
          deviceLanguageResolver: () => 'en',
        );
      },
      act: (cubit) => cubit.setLanguage(AppLanguage.turkish),
      expect: () => [AppLanguage.turkish],
    );

    test('writes Player.Locale in xx-XX form', () async {
      final writer = _RecordingLocaleWriter();
      final cubit = LocaleCubit(
        repository: InMemoryLocalePreferencesRepository(),
        localeWriter: writer,
        deviceLanguageResolver: () => 'en',
      );

      await cubit.setLanguage(AppLanguage.french);

      expect(writer.written, ['fr-FR']);
      await cubit.close();
    });

    test('no-ops when selecting the current language', () async {
      final writer = _RecordingLocaleWriter();
      final cubit = LocaleCubit(
        repository: InMemoryLocalePreferencesRepository(AppLanguage.turkish),
        localeWriter: writer,
        deviceLanguageResolver: () => 'tr',
      );
      await cubit.load();

      await cubit.setLanguage(AppLanguage.turkish);

      expect(writer.written, isEmpty);
      await cubit.close();
    });
  });

  group('AppLanguage', () {
    test('maps codes to region-qualified backend locales', () {
      expect(AppLanguage.turkish.backendLocale, 'tr-TR');
      expect(AppLanguage.english.backendLocale, 'en-US');
      expect(AppLanguage.german.backendLocale, 'de-DE');
      expect(AppLanguage.french.backendLocale, 'fr-FR');
      expect(AppLanguage.spanish.backendLocale, 'es-ES');
    });

    test('fromCode is region-insensitive and null for unknown', () {
      expect(AppLanguage.fromCode('de'), AppLanguage.german);
      expect(AppLanguage.fromCode('ja'), isNull);
      expect(AppLanguage.fromCode(null), isNull);
    });
  });
}
