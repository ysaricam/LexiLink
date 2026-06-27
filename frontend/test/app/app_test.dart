import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/app/app.dart';
import 'package:lexilink_app/features/settings/data/audio_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/color_palette_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/locale_preferences_repository.dart';
import 'package:lexilink_app/features/settings/data/player_locale_writer.dart';
import 'package:lexilink_app/features/splash/presentation/splash_screen.dart';
import 'package:lexilink_app/shared/ads/ads_platform.dart';
import 'package:lexilink_app/shared/ads/ads_service.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

class _UnsupportedAdsPlatform implements AdsPlatform {
  @override
  bool get isSupported => false;

  @override
  Future<void> gatherConsent() async {}

  @override
  Future<void> initialize() async {}

  @override
  Future<void> showInterstitial(String adUnitId) async {}

  @override
  Future<void> showRewarded({
    required String adUnitId,
    required String userId,
    required void Function() onClosed,
    required void Function() onUnavailable,
  }) async {}
}

void main() {
  testWidgets('renders splash screen on launch', (tester) async {
    await tester.pumpWidget(
      LexiLinkApp(
        audioService: AudioService(),
        adsService: AdsService(platform: _UnsupportedAdsPlatform()),
        audioPreferencesRepository: InMemoryAudioPreferencesRepository(),
        colorPalettePreferencesRepository:
            InMemoryColorPalettePreferencesRepository(),
        localePreferencesRepository: InMemoryLocalePreferencesRepository(),
        playerLocaleWriter: const NoopPlayerLocaleWriter(),
      ),
    );

    expect(find.byType(SplashScreen), findsOneWidget);
  });
}
