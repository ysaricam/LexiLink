import 'package:flutter_test/flutter_test.dart';
import 'package:lexilink_app/app/app.dart';
import 'package:lexilink_app/features/splash/presentation/splash_screen.dart';

void main() {
  testWidgets('renders splash screen on launch', (tester) async {
    await tester.pumpWidget(const LexiLinkApp());

    expect(find.byType(SplashScreen), findsOneWidget);
  });
}
