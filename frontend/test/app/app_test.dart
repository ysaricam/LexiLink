import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/app/app.dart';
import 'package:lexilink_app/features/splash/presentation/splash_screen.dart';
import 'package:lexilink_app/shared/audio/audio_service.dart';

void main() {
  testWidgets('renders splash screen on launch', (tester) async {
    await tester.pumpWidget(LexiLinkApp(audioService: AudioService()));

    expect(find.byType(SplashScreen), findsOneWidget);
  });
}
